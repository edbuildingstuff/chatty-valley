using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using ChattyValley.Core;
using ChattyValley.Runtime;

// Chatty Valley inference sidecar. Loads the base GGUF, validates and registers one LoRA per
// villager, then serves one request/response per line over a named pipe. On client connect it
// first sends a handshake line reporting per-adapter validation status; adapters load lazily on
// first use: validated at startup, loaded on demand, load failures remembered.
//
//   handshake (sidecar -> mod):  {"ready":true,"base":"<file>","adapters":[{"name","status","detail"}],
//                                 "gpu":{"requested","active","device","fallbackReason"}}
//   request   (mod -> sidecar):  {"prompt":"<chatml>","character":"Linus","temp":0.35,"maxTokens":96,
//                                 "repeatPenalty":1.1,"frequencyPenalty":0.1}   (penalties optional)
//   response  (sidecar -> mod):  {"reply":"...","raw":"..."}
//                            or  {"error":"unknown_character|adapter_load_failed|bad_request|...",
//                                 "character":"...","detail":"..."}
// The pipe server is created only AFTER the base model loads, so a successful client connect =
// base ready.
//
// Usage: ChattyValley.Sidecar --base <gguf> --pipe <name> [--gpu auto|on|off] [--gpu-layers N] --adapter <name>=<path> [--adapter <name>=<path> ...]

string? basePath = Arg("--base"), pipeName = Arg("--pipe");
// GPU resolution (DAT-704). Explicit --gpu-layers is a dev override that beats the mode; the mod
// always passes --gpu. Default off preserves the pre-0.4.0 bare-CLI behaviour.
int? explicitLayers = int.TryParse(Arg("--gpu-layers"), out var g) ? g : null;
string gpuMode = (Arg("--gpu") ?? GpuModes.Off).ToLowerInvariant();
if (gpuMode is not (GpuModes.Auto or GpuModes.On or GpuModes.Off)) gpuMode = GpuModes.Off;

GpuAdapterInfo? chosenGpu = null;
int gpuLayers;
if (explicitLayers is int el) { gpuLayers = el; }
else if (gpuMode == GpuModes.On) { gpuLayers = 99; }
else if (gpuMode == GpuModes.Auto)
{
    chosenGpu = GpuEligibility.PickEligible(QueryVideoControllers());
    gpuLayers = chosenGpu is null ? 0 : 99;
}
else { gpuLayers = 0; }

// When resolution says CPU, load the CPU-only native build outright: a machine with a broken
// Vulkan runtime then never initialises it at all. Must run before the first native load.
if (gpuLayers == 0)
    try { LLama.Native.NativeLibraryConfig.All.WithVulkan(false); } catch { }

var adapterArgs = ArgAll("--adapter");
var adapters = new List<(string Name, string Path)>();
foreach (string a in adapterArgs)
{
    if (!AdapterArg.TryParse(a, out var name, out var path))
    {
        Console.Error.WriteLine($"sidecar: malformed --adapter '{a}', expected <name>=<path>");
        return 1;
    }
    adapters.Add((name, path));
}
if (basePath is null || pipeName is null || adapters.Count == 0)
{
    Console.Error.WriteLine("usage: --base <gguf> --pipe <name> [--gpu auto|on|off] [--gpu-layers N] --adapter <name>=<path> [--adapter <name>=<path> ...]");
    return 1;
}

// Keep llama.cpp's native chatter on stderr; the mod drains stdout/stderr and logs it at trace.
try { LLama.Native.NativeLogConfig.llama_log_set((level, msg) => Console.Error.Write(msg)); } catch { }

var llm = new LlmRuntime();
string? gpuFallbackReason = null;
try
{
    await llm.LoadBaseAsync(basePath, contextSize: 2048, gpuLayers: gpuLayers);
}
catch (Exception ex) when (gpuLayers > 0)
{
    // GPU load failed: self-heal on CPU (DAT-704). The Vulkan-build-at-0-layers path is the
    // DAT-703 fallback arm, verified clean. Only a CPU failure after this is fatal (exit 2),
    // which the mod's existing ExitedEarly handling covers.
    Console.Error.WriteLine("sidecar: GPU model load failed, retrying on CPU: " + ex);
    gpuFallbackReason = ex.Message.Split('\n')[0].Trim();
    await llm.DisposeAsync();
    llm = new LlmRuntime();
    gpuLayers = 0;
    try
    {
        await llm.LoadBaseAsync(basePath, contextSize: 2048, gpuLayers: 0);
    }
    catch (Exception cpuEx)
    {
        Console.Error.WriteLine("sidecar: model load failed: " + cpuEx);
        return 2;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine("sidecar: model load failed: " + ex);
    return 2;
}
await using var _llm = llm;   // restore the await-using disposal the old declaration had

// Validate every adapter file now (cheap), register only the ones that pass; the handshake
// carries the full report so the mod can exclude broken villagers and say why.
var statuses = new List<AdapterStatus>();
foreach (var (name, path) in adapters)
{
    var status = AdapterValidation.Check(name, path);
    statuses.Add(status);
    if (status.Status == AdapterStatuses.Ok) llm.RegisterAdapter(name, path);
}
var router = new AdapterRouter(statuses);

// Warm-up generations, discarded. A process's first generations pay one-time costs that would
// otherwise land on the player's first chat turns of the session: on CPU that is JIT and cache
// warm-up at a few hundred ms; on the Vulkan path it is compute-pipeline compilation, and the
// driver compiles PER KERNEL SHAPE, not once per process. A single tiny warm-up only compiles
// the shapes its own prompt uses: on an RTX 5070 Ti (NV_coopmat2) it left the first three
// realistic-length player turns paying 2-5 s each for the prompt-size buckets it never touched
// (trainer-machine probe, 2026-08-10; the RTX 2070's ~6.3 s compile covered the shapes that
// card uses, which is why one warm-up looked sufficient during the 0.4.0 smoke). So on GPU the
// warm-up walks ascending prompt lengths to cross every size bucket a real conversation hits,
// and it runs adapter-ACTIVE, because real requests infer with the LoRA applied on a fresh
// context and the LoRA matmuls bring kernels of their own; activating it here also takes the
// adapter's lazy first-load off the player's first turn. Compiled pipelines land in the
// driver's on-disk shader cache, so the full cost is paid once per machine; later sessions
// replay it from cache in well under a second. The pipe below is the mod's ready signal, so
// all of it stays inside the background startup the player never sees (the mod's connect
// timeout is 60 s; a cold-cache warm-up measures ~15 s on top of model load). Guarded: a
// warm-up failure is logged and skipped, leaving any real inference error to surface
// per-request exactly as it does today.
try
{
    var warmAdapter = statuses.Find(s => s.Status == AdapterStatuses.Ok);
    if (warmAdapter is not null) llm.SetActiveAdapter(warmAdapter.Name);
    int[] warmSizes = gpuLayers > 0 ? new[] { 8, 48, 160, 384, 704 } : new[] { 8 };
    var warmSw = System.Diagnostics.Stopwatch.StartNew();
    foreach (int words in warmSizes)
    {
        var passSw = System.Diagnostics.Stopwatch.StartNew();
        await foreach (var _ in llm.InferStreamAsync(WarmPrompt(words), 0.35f, 8)) { }
        Console.Error.WriteLine($"sidecar: warm-up pass ({words} words) done in {passSw.ElapsedMilliseconds} ms");
    }
    llm.SetActiveAdapter(null);
    Console.Error.WriteLine($"sidecar: warm-up generation done in {warmSw.ElapsedMilliseconds} ms");
}
catch (Exception ex)
{
    Console.Error.WriteLine("sidecar: warm-up generation failed (continuing): " + ex.Message);
}

using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
Console.Error.WriteLine($"sidecar: model ready, waiting for connection on pipe {pipeName}");
await server.WaitForConnectionAsync();
using var reader = new StreamReader(server, Encoding.UTF8);
using var writer = new StreamWriter(server, new UTF8Encoding(false)) { AutoFlush = true };

var gpuStatus = new GpuStatus(gpuMode, Active: gpuLayers > 0,
    Device: gpuLayers > 0 ? (chosenGpu?.Name ?? "requested by config") : null,
    FallbackReason: gpuFallbackReason);

await writer.WriteLineAsync(JsonSerializer.Serialize(
    new SidecarHandshake(true, Path.GetFileName(basePath), statuses, gpuStatus), SidecarJson.Options));

string? line;
while ((line = await reader.ReadLineAsync()) != null)
{
    try
    {
        var req = JsonSerializer.Deserialize<SidecarRequest>(line, SidecarJson.Options);
        if (req?.Prompt is null)
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(
                new { error = SidecarErrors.BadRequest }, SidecarJson.Options));
            continue;
        }
        if (!router.TryRoute(req.Character, out var errorCode, out var errorDetail))
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(
                new { error = errorCode, character = req.Character, detail = errorDetail },
                SidecarJson.Options));
            continue;
        }
        try
        {
            llm.SetActiveAdapter(req.Character!);   // lazy: loads the GGUF on first activation
        }
        catch (Exception ex)
        {
            router.MarkLoadFailed(req.Character!, ex.Message);
            await writer.WriteLineAsync(JsonSerializer.Serialize(
                new { error = SidecarErrors.AdapterLoadFailed, character = req.Character, detail = ex.Message },
                SidecarJson.Options));
            continue;
        }
        var sb = new StringBuilder();
        await foreach (var tok in llm.InferStreamAsync(req.Prompt, req.Temp, req.MaxTokens,
                           req.RepeatPenalty ?? 1.1f, req.FrequencyPenalty ?? 0.1f))
            sb.Append(tok);
        string raw = sb.ToString().Trim();
        await writer.WriteLineAsync(JsonSerializer.Serialize(
            new { reply = CollapseWordRuns(raw), raw }, SidecarJson.Options));
    }
    catch (Exception ex)
    {
        try
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(
                new { error = ex.Message }, SidecarJson.Options));
        }
        catch { }
    }
}
return 0;

static string? Arg(string name)
{
    var a = Environment.GetCommandLineArgs();
    for (int i = 1; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
    return null;
}

// Filler that tokenizes to roughly one token per word. The exact counts do not matter, only
// that the ladder of lengths crosses every kernel-size bucket a real conversation can hit.
static string WarmPrompt(int words)
{
    string[] filler = { "The", "seasons", "turn", "quietly", "in", "the", "valley." };
    var sb = new StringBuilder();
    for (int i = 0; i < words; i++) sb.Append(filler[i % filler.Length]).Append(' ');
    return sb.ToString();
}

static List<string> ArgAll(string name)
{
    var a = Environment.GetCommandLineArgs();
    var values = new List<string>();
    for (int i = 1; i < a.Length - 1; i++) if (a[i] == name) values.Add(a[i + 1]);
    return values;
}

// WMI is the OS's own inventory; a machine where the query throws is treated as having no
// eligible GPU, which lands on the proven CPU path.
static List<ChattyValley.Core.GpuAdapterInfo> QueryVideoControllers()
{
    var found = new List<ChattyValley.Core.GpuAdapterInfo>();
    try
    {
        using var searcher = new System.Management.ManagementObjectSearcher(
            "SELECT PNPDeviceID, Name, AdapterRAM FROM Win32_VideoController");
        foreach (var mo in searcher.Get())
            found.Add(new ChattyValley.Core.GpuAdapterInfo(
                mo["PNPDeviceID"] as string,
                mo["Name"] as string,
                mo["AdapterRAM"] is null ? 0L : Convert.ToInt64(mo["AdapterRAM"])));
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("sidecar: WMI video controller query failed, staying on CPU: " + ex.Message);
    }
    return found;
}

// Last-line-of-defense degeneration guard: collapse the same word repeated 3+ times in a row
// ("once once once once" -> "once"). Sampling penalties make this rare; this catches the stragglers
// so a degenerate reply never reaches the player. Deliberate doubles ("no, no") have punctuation
// between them and are untouched.
static string CollapseWordRuns(string text) =>
    System.Text.RegularExpressions.Regex.Replace(
        text, @"\b(\w+)(?:\s+\1\b){2,}", "$1",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

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
//   handshake (sidecar -> mod):  {"ready":true,"base":"<file>","adapters":[{"name","status","detail"}]}
//   request   (mod -> sidecar):  {"prompt":"<chatml>","character":"Linus","temp":0.35,"maxTokens":96,
//                                 "repeatPenalty":1.1,"frequencyPenalty":0.1}   (penalties optional)
//   response  (sidecar -> mod):  {"reply":"...","raw":"..."}
//                            or  {"error":"unknown_character|adapter_load_failed|bad_request|...",
//                                 "character":"...","detail":"..."}
// The pipe server is created only AFTER the base model loads, so a successful client connect =
// base ready.
//
// Usage: ChattyValley.Sidecar --base <gguf> --pipe <name> [--gpu-layers N] --adapter <name>=<path> [--adapter <name>=<path> ...]

string? basePath = Arg("--base"), pipeName = Arg("--pipe");
int gpuLayers = int.TryParse(Arg("--gpu-layers"), out var g) ? g : 0;
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
    Console.Error.WriteLine("usage: --base <gguf> --pipe <name> [--gpu-layers N] --adapter <name>=<path> [--adapter <name>=<path> ...]");
    return 1;
}

// Keep llama.cpp's native chatter on stderr; the mod drains stdout/stderr and logs it at trace.
try { LLama.Native.NativeLogConfig.llama_log_set((level, msg) => Console.Error.Write(msg)); } catch { }

await using var llm = new LlmRuntime();
try
{
    await llm.LoadBaseAsync(basePath, contextSize: 2048, gpuLayers: gpuLayers);
}
catch (Exception ex)
{
    Console.Error.WriteLine("sidecar: model load failed: " + ex);
    return 2;
}

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

using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
Console.Error.WriteLine($"sidecar: model ready, waiting for connection on pipe {pipeName}");
await server.WaitForConnectionAsync();
using var reader = new StreamReader(server, Encoding.UTF8);
using var writer = new StreamWriter(server, new UTF8Encoding(false)) { AutoFlush = true };

await writer.WriteLineAsync(JsonSerializer.Serialize(
    new SidecarHandshake(true, Path.GetFileName(basePath), statuses), SidecarJson.Options));

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

static List<string> ArgAll(string name)
{
    var a = Environment.GetCommandLineArgs();
    var values = new List<string>();
    for (int i = 1; i < a.Length - 1; i++) if (a[i] == name) values.Add(a[i + 1]);
    return values;
}

// Last-line-of-defense degeneration guard: collapse the same word repeated 3+ times in a row
// ("once once once once" -> "once"). Sampling penalties make this rare; this catches the stragglers
// so a degenerate reply never reaches the player. Deliberate doubles ("no, no") have punctuation
// between them and are untouched.
static string CollapseWordRuns(string text) =>
    System.Text.RegularExpressions.Regex.Replace(
        text, @"\b(\w+)(?:\s+\1\b){2,}", "$1",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

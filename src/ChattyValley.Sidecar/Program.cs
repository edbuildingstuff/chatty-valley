using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using ChattyValley.Runtime;

// Chatty Valley inference sidecar. Loads the base GGUF + a villager LoRA, then serves one
// request/response per line over a named pipe:
//   request  (mod -> sidecar):  {"prompt":"<full chatml prompt>","temp":0.6,"maxTokens":96}
//   response (sidecar -> mod):  {"reply":"..."}  or  {"error":"..."}
// The pipe server is created only AFTER the model loads, so a successful client connect = ready.
//
// Usage: ChattyValley.Sidecar --base <gguf> --adapter <gguf> --pipe <name> [--gpu-layers N]

string? basePath = Arg("--base"), adapterPath = Arg("--adapter"), pipeName = Arg("--pipe");
int gpuLayers = int.TryParse(Arg("--gpu-layers"), out var g) ? g : 0;
if (basePath is null || adapterPath is null || pipeName is null)
{
    Console.Error.WriteLine("usage: --base <gguf> --adapter <gguf> --pipe <name> [--gpu-layers N]");
    return 1;
}

// Keep llama.cpp's native chatter on stderr; the mod drains stdout/stderr and logs it at trace.
try { LLama.Native.NativeLogConfig.llama_log_set((level, msg) => Console.Error.Write(msg)); } catch { }

await using var llm = new LlmRuntime();
try
{
    await llm.LoadBaseAsync(basePath, contextSize: 2048, gpuLayers: gpuLayers);
    llm.RegisterAdapter("villager", adapterPath);
    llm.SetActiveAdapter("villager");
}
catch (Exception ex)
{
    Console.Error.WriteLine("sidecar: model load failed: " + ex);
    return 2;
}

var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
Console.Error.WriteLine($"sidecar: model ready, waiting for connection on pipe {pipeName}");
await server.WaitForConnectionAsync();
using var reader = new StreamReader(server, Encoding.UTF8);
using var writer = new StreamWriter(server, new UTF8Encoding(false)) { AutoFlush = true };

string? line;
while ((line = await reader.ReadLineAsync()) != null)
{
    try
    {
        var req = JsonSerializer.Deserialize<Req>(line, opts);
        if (req?.Prompt is null) { await writer.WriteLineAsync("{\"error\":\"bad request\"}"); continue; }
        var sb = new StringBuilder();
        await foreach (var tok in llm.InferStreamAsync(req.Prompt, req.Temp, req.MaxTokens))
            sb.Append(tok);
        await writer.WriteLineAsync(JsonSerializer.Serialize(new { reply = sb.ToString().Trim() }));
    }
    catch (Exception ex)
    {
        try { await writer.WriteLineAsync(JsonSerializer.Serialize(new { error = ex.Message })); } catch { }
    }
}
return 0;

static string? Arg(string name)
{
    var a = Environment.GetCommandLineArgs();
    for (int i = 1; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
    return null;
}

sealed record Req(string? Prompt, float Temp, int MaxTokens);

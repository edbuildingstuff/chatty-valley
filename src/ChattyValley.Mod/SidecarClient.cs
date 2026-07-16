using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using StardewModdingAPI;

namespace ChattyValley.Mod;

/// <summary>
/// Spawns and talks to the out-of-process inference sidecar (ChattyValley.Sidecar) over a named pipe.
/// The mod does no inference itself; this keeps the mod free of LLamaSharp's .NET 10 dependencies so
/// it loads in the .NET 6 game. One request/response at a time (the mod is single-flight).
/// </summary>
public sealed class SidecarClient : IDisposable
{
    private readonly IMonitor _monitor;
    private Process? _proc;
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public bool Ready { get; private set; }

    public SidecarClient(IMonitor monitor) => _monitor = monitor;

    /// <summary>Start the sidecar and connect once its model has loaded. Throws on failure.</summary>
    public async Task StartAsync(string sidecarExe, string basePath, string adapterPath, int gpuLayers)
    {
        string pipeName = "ChattyValley." + Guid.NewGuid().ToString("N");
        var psi = new ProcessStartInfo
        {
            FileName = sidecarExe,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var a in new[] { "--base", basePath, "--adapter", adapterPath, "--pipe", pipeName,
                                  "--gpu-layers", gpuLayers.ToString() })
            psi.ArgumentList.Add(a);

        _proc = Process.Start(psi) ?? throw new Exception("failed to start sidecar process");
        // Drain the sidecar's stdout/stderr (llama.cpp is chatty) so its pipes never fill and block it.
        _ = Drain(_proc.StandardOutput);
        _ = Drain(_proc.StandardError);

        _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await _pipe.ConnectAsync(60_000);   // the sidecar creates the pipe only after the model loads
        _reader = new StreamReader(_pipe, Encoding.UTF8);
        _writer = new StreamWriter(_pipe, new UTF8Encoding(false)) { AutoFlush = true };
        Ready = true;
    }

    /// <summary>Send one prompt, get one reply. Returns null on any error (caller shows nothing).</summary>
    public async Task<string?> AskAsync(string prompt, float temp, int maxTokens,
        float repeatPenalty, float frequencyPenalty)
    {
        if (!Ready || _writer is null || _reader is null) return null;
        try
        {
            await _writer.WriteLineAsync(JsonSerializer.Serialize(
                new { prompt, temp, maxTokens, repeatPenalty, frequencyPenalty }));
            string? line = await _reader.ReadLineAsync();
            if (line is null) return null;
            using var doc = JsonDocument.Parse(line);
            if (doc.RootElement.TryGetProperty("reply", out var r)) return r.GetString();
            if (doc.RootElement.TryGetProperty("error", out var e))
                _monitor.Log($"sidecar error: {e.GetString()}", LogLevel.Warn);
            return null;
        }
        catch (Exception ex)
        {
            _monitor.Log($"sidecar request failed (chat disabled this turn): {ex.Message}", LogLevel.Warn);
            Ready = false;
            return null;
        }
    }

    private static async Task Drain(StreamReader s)
    {
        try { while (await s.ReadLineAsync() is not null) { } } catch { /* process ended */ }
    }

    public void Dispose()
    {
        Ready = false;
        try { _pipe?.Dispose(); } catch { }
        try { if (_proc is { HasExited: false }) _proc.Kill(true); } catch { }
        try { _proc?.Dispose(); } catch { }
    }
}

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using ChattyValley.Core;
using StardewModdingAPI;

namespace ChattyValley.Mod;

/// <summary>
/// Thrown when the sidecar could not be started and handshaken. Carries which failure class it
/// was (DAT-681 distinguishes them in the console) and the tail of the sidecar's stderr, which is
/// where llama.cpp and .NET host errors actually land.
/// </summary>
public sealed class SidecarStartException : Exception
{
    public SidecarFailureKind Kind { get; }
    public IReadOnlyList<string> StderrTail { get; }
    public SidecarStartException(SidecarFailureKind kind, string message, IReadOnlyList<string> stderrTail)
        : base(message)
    {
        Kind = kind;
        StderrTail = stderrTail;
    }
}

/// <summary>
/// Spawns and talks to the out-of-process inference sidecar (ChattyValley.Sidecar) over a named
/// pipe. The mod does no inference itself; this keeps the mod free of LLamaSharp's .NET 10
/// dependencies so it loads in the .NET 6 game. One request/response at a time (the mod is
/// single-flight). One sidecar serves every villager: the roster is passed at start as repeated
/// --adapter name=path args, the sidecar reports per-adapter validation in a handshake line, and
/// each request names its character.
/// </summary>
public sealed class SidecarClient : IDisposable
{
    private const int StderrTailLines = 40;

    private readonly IMonitor _monitor;
    private Process? _proc;
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private readonly object _errLock = new();
    private readonly Queue<string> _stderrTail = new();

    public bool Ready { get; private set; }

    /// <summary>Per-adapter validation results from the handshake. Empty until started.</summary>
    public IReadOnlyList<AdapterStatus> AdapterStatuses { get; private set; } = Array.Empty<AdapterStatus>();

    /// <summary>GPU outcome from the handshake; null until started (or from a pre-0.4.0 sidecar).</summary>
    public GpuStatus? Gpu { get; private set; }

    public SidecarClient(IMonitor monitor) => _monitor = monitor;

    /// <summary>
    /// Start the sidecar with the whole roster and connect once its base model has loaded.
    /// Throws <see cref="SidecarStartException"/> with the failure class on any failure.
    /// </summary>
    public async Task StartAsync(string sidecarExe, string basePath,
        IReadOnlyCollection<RosterEntry> roster, string gpuMode)
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
        psi.ArgumentList.Add("--base"); psi.ArgumentList.Add(basePath);
        psi.ArgumentList.Add("--pipe"); psi.ArgumentList.Add(pipeName);
        psi.ArgumentList.Add("--gpu"); psi.ArgumentList.Add(gpuMode);
        foreach (var entry in roster)
        {
            psi.ArgumentList.Add("--adapter");
            psi.ArgumentList.Add($"{entry.Character.Name}={entry.AdapterFullPath}");
        }

        try
        {
            _proc = Process.Start(psi) ?? throw new SidecarStartException(
                SidecarFailureKind.LaunchFailed, "Process.Start returned null", Tail());
        }
        catch (SidecarStartException) { throw; }
        catch (Exception ex)
        {
            // Win32Exception here is the "Windows refused to run it" path DAT-681 cares about.
            throw new SidecarStartException(SidecarFailureKind.LaunchFailed, ex.Message, Tail());
        }
        _ = Drain(_proc.StandardOutput, keepTail: false);
        _ = Drain(_proc.StandardError, keepTail: true);

        // Connect in short slices so an early process death is reported as ExitedEarly with its
        // exit code, immediately, instead of as a generic timeout 60 seconds later.
        _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (true)
        {
            if (_proc.HasExited)
                throw new SidecarStartException(SidecarFailureKind.ExitedEarly,
                    $"sidecar exited with code {_proc.ExitCode} before the pipe connected", Tail());
            try
            {
                await _pipe.ConnectAsync(2_000);   // the sidecar creates the pipe only after the model loads
                break;
            }
            catch (TimeoutException)
            {
                if (DateTime.UtcNow >= deadline)
                    throw new SidecarStartException(SidecarFailureKind.ConnectTimeout,
                        "sidecar did not open its pipe within 60s", Tail());
            }
        }
        _reader = new StreamReader(_pipe, Encoding.UTF8);
        _writer = new StreamWriter(_pipe, new UTF8Encoding(false)) { AutoFlush = true };

        string? hsLine = await _reader.ReadLineAsync();
        SidecarHandshake? hs = null;
        if (hsLine is not null)
        {
            try { hs = JsonSerializer.Deserialize<SidecarHandshake>(hsLine, SidecarJson.Options); }
            catch { /* fall through to the null check */ }
        }
        if (hs is null || !hs.Ready)
            throw new SidecarStartException(SidecarFailureKind.ConnectTimeout,
                "sidecar connected but sent no valid handshake", Tail());
        AdapterStatuses = hs.Adapters;
        Gpu = hs.Gpu;
        Ready = true;
    }

    /// <summary>
    /// Send one prompt for one character, get one reply. Reply is null on any error; Error then
    /// carries the sidecar's error code (see <see cref="SidecarErrors"/>) or a transport message.
    /// Raw is the model's verbatim output before the word-run collapse guard, recorded so
    /// degeneration events stay visible in the chat log.
    /// </summary>
    public async Task<(string? Reply, string? Raw, string? Error)> AskDetailedAsync(string prompt,
        string character, float temp, int maxTokens, float repeatPenalty, float frequencyPenalty)
    {
        if (!Ready || _writer is null || _reader is null) return (null, null, "sidecar not ready");
        try
        {
            var req = new SidecarRequest(prompt, character, temp, maxTokens, repeatPenalty, frequencyPenalty);
            await _writer.WriteLineAsync(JsonSerializer.Serialize(req, SidecarJson.Options));
            string? line = await _reader.ReadLineAsync();
            if (line is null) return (null, null, "pipe closed");
            using var doc = JsonDocument.Parse(line);
            if (doc.RootElement.TryGetProperty("reply", out var r))
            {
                string? raw = doc.RootElement.TryGetProperty("raw", out var w) ? w.GetString() : null;
                return (r.GetString(), raw, null);
            }
            if (doc.RootElement.TryGetProperty("error", out var e))
            {
                string? detail = doc.RootElement.TryGetProperty("detail", out var d) ? d.GetString() : null;
                _monitor.Log($"sidecar error: {e.GetString()} {detail}", LogLevel.Warn);
                return (null, null, e.GetString());
            }
            return (null, null, "unrecognised sidecar response");
        }
        catch (Exception ex)
        {
            _monitor.Log($"sidecar request failed (chat disabled this turn): {ex.Message}. " +
                         "See the SMAPI log above for sidecar output.", LogLevel.Warn);
            Ready = false;
            return (null, null, ex.Message);
        }
    }

    private string[] Tail()
    {
        lock (_errLock) return _stderrTail.ToArray();
    }

    private async Task Drain(StreamReader s, bool keepTail)
    {
        try
        {
            string? line;
            while ((line = await s.ReadLineAsync()) is not null)
            {
                if (!keepTail) continue;
                lock (_errLock)
                {
                    _stderrTail.Enqueue(line);
                    while (_stderrTail.Count > StderrTailLines) _stderrTail.Dequeue();
                }
            }
        }
        catch { /* process ended */ }
    }

    public void Dispose()
    {
        Ready = false;
        try { _pipe?.Dispose(); } catch { }
        try { if (_proc is { HasExited: false }) _proc.Kill(true); } catch { }
        try { _proc?.Dispose(); } catch { }
    }
}

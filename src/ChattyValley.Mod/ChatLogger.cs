using System.Text.Json;

namespace ChattyValley.Mod;

/// <summary>
/// Development chat-transcript log: appends one JSON object per event (conversation start, each
/// turn, conversation end) to a per-day JSONL file, so in-game test conversations can be replayed
/// and debugged after a play-through (see tools/read_chatlog.py in the repo for a pretty-printer).
///
/// Records are self-describing: the start event carries the game context, model files, and sampling
/// settings that produced the conversation, and each turn carries the player line, the shown reply,
/// the model's verbatim pre-guard output when it differed, and latency. Logging must never affect
/// gameplay: every write is wrapped and a failure silently disables nothing but itself.
/// </summary>
internal sealed class ChatLogger
{
    private readonly string _path;
    private readonly object _lock = new();

    public ChatLogger(string dir)
    {
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, $"chatlog-{DateTime.Now:yyyy-MM-dd}.jsonl");
    }

    public string FilePath => _path;

    public void Write(object record)
    {
        try
        {
            string line = JsonSerializer.Serialize(record,
                new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            lock (_lock)
                File.AppendAllText(_path, line + Environment.NewLine);
        }
        catch
        {
            // Logging is best-effort by design; chat must keep working even if the disk does not.
        }
    }

    public static string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}

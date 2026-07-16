using StardewModdingAPI;

namespace ChattyValley.Mod;

/// <summary>Player-editable config (config.json, written by SMAPI on first run).</summary>
public sealed class ModConfig
{
    /// <summary>Key that opens free-chat with the villager you are facing (when it is safe to).</summary>
    public SButton ChatKey { get; set; } = SButton.C;

    /// <summary>After a villager's canonical dialogue closes, surface a small "keep talking" hint.</summary>
    public bool ShowContinuationHint { get; set; } = true;

    /// <summary>Sampling temperature and reply cap for the local model.</summary>
    public float Temperature { get; set; } = 0.6f;
    public int MaxTokens { get; set; } = 96;

    /// <summary>
    /// Anti-repetition sampling. A small character model can loop a favourite word deep into a
    /// conversation without these; 1.0 / 0.0 turns them off entirely.
    /// </summary>
    public float RepeatPenalty { get; set; } = 1.1f;
    public float FrequencyPenalty { get; set; } = 0.1f;

    /// <summary>
    /// How many recent chat messages (player + villager combined) are sent to the model each turn.
    /// Older messages fall out of the window: the adapter is trained on short conversations, and an
    /// ever-growing history both drifts out of distribution and slows CPU inference.
    /// </summary>
    public int MaxHistoryMessages { get; set; } = 12;

    /// <summary>
    /// Development transcript logging: every free-chat conversation is appended as JSONL (context,
    /// model + sampling settings, each player turn and reply, pre-guard raw output, latency) so
    /// test sessions can be reviewed after a play-through. Written under ChatLogDir (blank = the
    /// mod folder's chat-logs/); pretty-print with tools/read_chatlog.py from the repo.
    /// </summary>
    public bool ChatLogEnabled { get; set; } = true;

    /// <summary>Also log the full rendered prompt each turn (verbose; deep prompt-debugging only).</summary>
    public bool ChatLogPrompts { get; set; } = false;

    /// <summary>Directory for chat logs. Blank = "chat-logs" inside the mod folder.</summary>
    public string ChatLogDir { get; set; } = "";

    /// <summary>
    /// Absolute paths to the GGUF files. Left blank = resolve from the repo's models/ folder (dev) and
    /// the trained adapter. Set explicitly for a deployed install. The adapter path can point anywhere.
    /// </summary>
    public string BaseModelPath { get; set; } = "";
    public string LinusAdapterPath { get; set; } = "";

    /// <summary>GPU layers to offload (0 = CPU only, the shipping default so it runs on any machine).</summary>
    public int GpuLayers { get; set; } = 0;
}

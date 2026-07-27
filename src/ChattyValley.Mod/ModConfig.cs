using ChattyValley.Core;
using StardewModdingAPI;

namespace ChattyValley.Mod;

/// <summary>Player-editable config (config.json, written by SMAPI on first run).</summary>
public sealed class ModConfig
{
    /// <summary>Key that opens free-chat with the villager you are facing (when it is safe to).</summary>
    public SButton ChatKey { get; set; } = SButton.C;

    /// <summary>After a villager's canonical dialogue closes, surface a small "keep talking" hint.</summary>
    public bool ShowContinuationHint { get; set; } = true;

    /// <summary>
    /// 0.35 is the validated decode regime for the shipping adapter. Raising it measurably increases
    /// the model's tendency to play along with false premises.
    /// </summary>
    public float Temperature { get; set; } = 0.35f;
    public int MaxTokens { get; set; } = 96;

    /// <summary>
    /// Anti-repetition sampling. A small character model can loop a favourite word deep into a
    /// conversation without these; 1.0 / 0.0 turns them off entirely.
    /// </summary>
    public float RepeatPenalty { get; set; } = 1.1f;
    public float FrequencyPenalty { get; set; } = 0.1f;

    /// <summary>
    /// When the player says an explicit goodbye ("bye", "gotta go"), or the model's reply carries
    /// the trained end-of-conversation marker, that reply is the LAST one: dismissing its dialogue
    /// box ends the conversation instead of reopening the input bar. The vanilla click-to-dismiss
    /// is the exit, so there is no timer. Esc always ends immediately either way.
    /// </summary>
    public bool AutoCloseOnFarewell { get; set; } = true;

    /// <summary>
    /// How many recent chat messages (player + villager combined) are sent to the model each turn.
    /// Older messages fall out of the window: the adapter is trained on short conversations, and an
    /// ever-growing history both drifts out of distribution and slows CPU inference.
    /// </summary>
    public int MaxHistoryMessages { get; set; } = 12;

    /// <summary>
    /// False-premise guard. When the player's message presupposes a fabricated event, gift, or shared
    /// past ("when Leah visited your tent, what did you talk about?"), append <see cref="FalsePremiseGuardClause"/>
    /// to the system turn so the model is reminded to say plainly if it does not remember, instead of
    /// playing along. Fires only on those presupposition shapes (never on ordinary chat or relationship
    /// questions), so normal conversation is untouched.
    /// </summary>
    public bool FalsePremiseGuard { get; set; } = true;
    public string FalsePremiseGuardClause { get; set; } = ConversationSignals.DefaultFalsePremiseGuard;

    /// <summary>
    /// Development transcript logging, off by default in shipped builds. When enabled, every free-chat
    /// conversation is appended as JSONL (context,
    /// model + sampling settings, each player turn and reply, pre-guard raw output, latency) so
    /// test sessions can be reviewed after a play-through. Written under ChatLogDir (blank = the
    /// mod folder's chat-logs/); pretty-print with tools/read_chatlog.py from the repo.
    /// </summary>
    public bool ChatLogEnabled { get; set; } = false;

    /// <summary>Also log the full rendered prompt each turn (verbose; deep prompt-debugging only).</summary>
    public bool ChatLogPrompts { get; set; } = false;

    /// <summary>Directory for chat logs. Blank = "chat-logs" inside the mod folder.</summary>
    public string ChatLogDir { get; set; } = "";

    /// <summary>
    /// Absolute paths to the GGUF files. Left blank = resolve to the mod folder's own `assets/`
    /// subfolder (see <c>ModEntry.Resolve</c>), which is where <c>scripts/package-release.ps1</c>
    /// stages the shipping GGUFs and where a dev build can drop them too. Set explicitly for a
    /// deployed install. The adapter path can point anywhere.
    /// </summary>
    public string BaseModelPath { get; set; } = "";
    public string LinusAdapterPath { get; set; } = "";

    /// <summary>GPU layers to offload (0 = CPU only, the shipping default so it runs on any machine).</summary>
    public int GpuLayers { get; set; } = 0;
}

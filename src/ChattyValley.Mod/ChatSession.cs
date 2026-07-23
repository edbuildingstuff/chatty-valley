using ChattyValley.Core;
using StardewValley;
using StardewValley.Menus;

namespace ChattyValley.Mod;

/// <summary>
/// One free-chat conversation, displayed through the game's own conversation UI so it reads like
/// any other villager dialogue (Edward's direction, 2026-07-23): the villager's replies appear in
/// the VANILLA <see cref="DialogueBox"/> (portrait, typewriter reveal, click to dismiss), and a
/// slim <see cref="ChatInputBar"/> opens between replies for the player's next line.
///
/// State machine: Thinking (input bar shows "...", model generating) -> ShowingReply (vanilla
/// DialogueBox up) -> AwaitingInput (input bar, typing) -> Thinking -> ... A farewell (player
/// heuristic or trained [end] marker) makes the current reply the LAST one: dismissing that
/// dialogue box ends the conversation instead of reopening the input bar, so the vanilla
/// click-to-dismiss is itself the exit affordance; no timers.
///
/// The session never touches canonical state: the Dialogue object is built standalone (the NPC's
/// CurrentDialogue stack is never pushed to), the same read-only contract as ModEntry declares.
/// </summary>
public sealed class ChatSession
{
    private enum State { Thinking, ShowingReply, AwaitingInput, Ended }

    private readonly NPC _npc;
    private readonly Func<IReadOnlyList<ChatTurn>, Task<string?>> _ask;
    private readonly bool _autoCloseOnFarewell;
    private readonly Action<string> _onEnd;                 // chat-log end hook (reason)
    private readonly List<ChatTurn> _history = new();

    private State _state = State.Thinking;
    private bool _endAfterReply;
    private string _endReason = "esc";
    private volatile string? _pendingReply;
    private DialogueBox? _dialogueBox;                      // the box we opened, to recognise its close

    public ChatSession(NPC npc, Func<IReadOnlyList<ChatTurn>, Task<string?>> ask,
        bool autoCloseOnFarewell, Action<string> onEnd)
    {
        _npc = npc;
        _ask = ask;
        _autoCloseOnFarewell = autoCloseOnFarewell;
        _onEnd = onEnd;
    }

    public bool IsAlive => _state != State.Ended;

    /// <summary>Open the conversation: input bar in thinking state while the greeting generates.</summary>
    public void Start()
    {
        _history.Add(new ChatTurn(true, "Hello, " + _npc.Name + "."));
        Generate();
        Game1.activeClickableMenu = new ChatInputBar(this, _npc);
    }

    // ---- input bar callbacks --------------------------------------------------------------------

    public bool Thinking => _state == State.Thinking;

    /// <summary>Player typed a line and pressed Enter.</summary>
    public void Submit(string text)
    {
        if (_state != State.AwaitingInput) return;
        if (_autoCloseOnFarewell && ConversationSignals.IsPlayerFarewell(text))
        {
            _endAfterReply = true;
            _endReason = "player-farewell";
        }
        _history.Add(new ChatTurn(true, text));
        Generate();
    }

    /// <summary>Esc from the input bar: the player left without a goodbye.</summary>
    public void Abort()
    {
        if (_state == State.Ended) return;
        _state = State.Ended;
        _onEnd("esc");
    }

    /// <summary>
    /// Called from the input bar's update tick. When the pending reply lands, swap the input bar
    /// for the vanilla DialogueBox. Runs on the main thread, like all menu changes.
    /// </summary>
    public void PumpPendingReply()
    {
        if (_state != State.Thinking || _pendingReply is not { } reply) return;
        _pendingReply = null;

        bool modelEnd = ConversationSignals.TryStripEndMarker(reply, out string cleaned);
        if (_autoCloseOnFarewell && modelEnd)
        {
            _endAfterReply = true;
            _endReason = "model-end-marker";
        }
        _history.Add(new ChatTurn(false, cleaned));         // canonical: marker stripped, "@" intact

        string shown = DialogueTextSanitizer.Sanitize(cleaned, Game1.player.Name);
        _state = State.ShowingReply;
        _dialogueBox = new DialogueBox(new Dialogue(_npc, null, shown));
        Game1.activeClickableMenu = _dialogueBox;
    }

    // ---- menu-close routing (from ModEntry.OnMenuChanged) ---------------------------------------

    /// <summary>
    /// The game closed a menu. If it was our DialogueBox, either reopen the input bar for the next
    /// line or, after a farewell, end the conversation; the click that dismissed the box was the
    /// natural exit. Any foreign menu appearing (event, festival cutscene) ends the session quietly.
    /// </summary>
    public void OnMenuChanged(IClickableMenu? oldMenu, IClickableMenu? newMenu)
    {
        if (_state == State.Ended) return;

        if (oldMenu == _dialogueBox && _state == State.ShowingReply)
        {
            _dialogueBox = null;
            if (newMenu != null) { EndQuietly("interrupted"); return; }
            if (_endAfterReply)
            {
                _state = State.Ended;
                _onEnd(_endReason);
                return;
            }
            _state = State.AwaitingInput;
            Game1.activeClickableMenu = new ChatInputBar(this, _npc);
            return;
        }

        // Something scripted replaced one of our menus (an event stealing the screen): stand down.
        if (newMenu != null && newMenu is not ChatInputBar && newMenu != _dialogueBox
            && oldMenu is ChatInputBar)
            EndQuietly("interrupted");
    }

    private void EndQuietly(string reason)
    {
        _state = State.Ended;
        _onEnd(reason);
    }

    // ---- generation -----------------------------------------------------------------------------

    private void Generate()
    {
        _state = State.Thinking;
        _pendingReply = null;
        var snapshot = _history.ToArray();
        _ = Task.Run(async () =>
        {
            string? reply = await _ask(snapshot);
            _pendingReply = string.IsNullOrWhiteSpace(reply) ? "..." : reply!.Trim();
        });
    }
}

using System.Text;
using System.Text.Json;
using ChattyValley.Core;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
// Disambiguate: "Character" = our persona type (StardewValley also has a Character base class);
// "Mod" the type = SMAPI's base (this project's namespace is also named ...Mod).
using Character = ChattyValley.Core.Character;

namespace ChattyValley.Mod;

/// <summary>
/// Chatty Valley SMAPI entry point.
///
/// DESIGN CONTRACT (do not weaken): this mod is a READ-ONLY, ADDITIVE overlay. It never intercepts,
/// replaces, or pre-empts the game's own dialogue, and it never writes to canonical state (friendship,
/// quests, events, mail, "talked today", the save). Canonical / milestone conversations always play
/// first and untouched; free-chat is an opt-in layer the player invokes with a key, and only when the
/// villager is normally interactable and nothing scripted is happening. Close the chat and the game is
/// byte-identical to if it had never opened. If the model errors or lags, we do nothing and the game
/// proceeds vanilla. This is how "the mod cannot affect canonical gameplay" is guaranteed by construction.
///
/// STATUS: working end-to-end in-game. Press the chat key near Linus (when the vanilla dialogue is
/// closed and nothing scripted is happening) -> read live game state -> open a typed, multi-turn chat
/// window (ChatMenu) backed by the on-device model. Inference runs out-of-process in ChattyValley.Sidecar
/// (LLamaSharp 0.27.0 pins .NET 10 deps that cannot load in the .NET 6 game), reached over a named pipe.
/// Friendship-from-chat is deferred (open-ended in-game mechanic; decision locked later) and is a clean
/// no-op seam in OpenChat.
/// </summary>
public sealed class ModEntry : StardewModdingAPI.Mod
{
    private ModConfig _config = new();
    private SidecarClient? _sidecar;
    private Character? _linus;
    private PromptBuilder? _prompt;
    private ChatLogger? _chatLog;
    private string _baseModelFile = "";
    private string _adapterFile = "";

    private const string LinusName = "Linus";

    public override void Entry(IModHelper helper)
    {
        _config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        // Kill the sidecar if the game process exits (belt-and-suspenders; it also self-exits when the
        // pipe breaks on game close).
        AppDomain.CurrentDomain.ProcessExit += (_, _) => _sidecar?.Dispose();
        Monitor.Log("Chatty Valley loaded. Additive free-chat; canonical gameplay is never modified.", LogLevel.Info);
    }

    // ---- start the out-of-process inference sidecar (once, at launch; off the game thread) -------

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        _ = Task.Run(StartSidecarAsync);   // don't block the game; chat just isn't offered until ready
    }

    private async Task StartSidecarAsync()
    {
        try
        {
            string basePath = Resolve(_config.BaseModelPath, "LFM2.5-350M-Q4_K_M.gguf");
            string adapterPath = Resolve(_config.LinusAdapterPath, "linus-350m-v1-lora-f16.gguf");
            string sidecarExe = Path.Combine(Helper.DirectoryPath, "sidecar", "ChattyValley.Sidecar.exe");
            if (!File.Exists(basePath) || !File.Exists(adapterPath) || !File.Exists(sidecarExe))
            {
                Monitor.Log("Missing files, free-chat disabled (game unaffected). " +
                            $"base={basePath} adapter={adapterPath} sidecar={sidecarExe}", LogLevel.Warn);
                return;
            }

            _linus = LoadCharacter("linus.json", adapterPath);
            _prompt = new PromptBuilder(ChatTemplate.Lfm2);
            _baseModelFile = Path.GetFileName(basePath);
            _adapterFile = Path.GetFileName(adapterPath);

            if (_config.ChatLogEnabled)
            {
                string dir = string.IsNullOrWhiteSpace(_config.ChatLogDir)
                    ? Path.Combine(Helper.DirectoryPath, "chat-logs")
                    : _config.ChatLogDir;
                _chatLog = new ChatLogger(dir);
                Monitor.Log($"Chat transcript log: {_chatLog.FilePath}", LogLevel.Info);
            }

            _sidecar = new SidecarClient(Monitor);
            await _sidecar.StartAsync(sidecarExe, basePath, adapterPath, _config.GpuLayers);
            Monitor.Log("Local model + Linus adapter ready (via sidecar).", LogLevel.Info);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Sidecar/model start failed; free-chat disabled (game unaffected): {ex.Message}", LogLevel.Error);
        }
    }

    // ---- the chat key: the ONLY entry point, gated so canon is never touched ---------------------

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button != _config.ChatKey) return;
        if (_sidecar is not { Ready: true }) return;
        if (!TryGetChattableVillager(out NPC villager)) return;

        // Suppress this key so it can't also trigger a vanilla bound action this frame.
        Helper.Input.Suppress(e.Button);
        OpenChat(villager);
    }

    // After any conversation closes, if the player is still by a chattable Linus, nudge that free-chat
    // is available. Pure UI hint; touches no game state.
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (!_config.ShowContinuationHint || e.NewMenu != null) return;
        if (_sidecar is not { Ready: true }) return;
        if (!TryGetChattableVillager(out NPC linus)) return;
        Game1.addHUDMessage(new HUDMessage($"Press {_config.ChatKey} to keep talking with {linus.Name}", 2));
    }

    /// <summary>
    /// The safety gate. Returns a villager ONLY when free-chat is safe and the villager is normally
    /// interactable right now. Any doubt -> false, and the mod does nothing.
    /// </summary>
    private bool TryGetChattableVillager(out NPC villager)
    {
        villager = null!;
        // Not during anything scripted, forced, or menu-driven. IsPlayerFree already covers most of it.
        if (!Context.IsWorldReady || !Context.IsPlayerFree) return false;
        if (Game1.eventUp || Game1.isFestival() || Game1.currentMinigame != null) return false;
        if (Game1.activeClickableMenu != null) return false;
        var loc = Game1.currentLocation;
        if (loc == null) return false;

        // Find Linus standing near the player (forgiving: within ~2.5 tiles, no exact facing needed).
        // Only when he is normally socialisable, so plot-gated / off-map states are respected.
        Vector2 p = Game1.player.Tile;
        NPC? nearest = null;
        float best = 2.5f;
        foreach (var npc in loc.characters)
        {
            if (npc.IsVillager && npc.Name == LinusName && npc.CanSocialize)
            {
                float d = Vector2.Distance(npc.Tile, p);
                if (d <= best) { best = d; nearest = npc; }
            }
        }
        if (nearest == null) return false;
        villager = nearest;
        return true;
    }

    // Open the typed, multi-turn chat. A self-contained mod menu that reads no further game state and
    // writes nothing back; closing it (Esc) ends the conversation. The game context is snapshotted now,
    // so the whole conversation is grounded in the moment it began.
    // TODO(friendship, deferred): a later stage may award a little friendship for chatting (open-ended
    // in-game mechanic; decision locked later). Intentionally a no-op now so this stays read-only.
    private void OpenChat(NPC villager)
    {
        GameContext ctx = ReadContext(villager);
        string convo = Guid.NewGuid().ToString("N").Substring(0, 8);
        _chatLog?.Write(new
        {
            evt = "start", ts = ChatLogger.Timestamp(), convo, npc = villager.Name,
            gameTime = $"{ctx.Season} {ctx.Day} year {ctx.Year}, {ctx.Weekday} {ctx.Clock}",
            system = _prompt!.BuildSystem(_linus!, ctx),
            baseModel = _baseModelFile, adapter = _adapterFile,
            temp = _config.Temperature, maxTokens = _config.MaxTokens,
            repeatPenalty = _config.RepeatPenalty, frequencyPenalty = _config.FrequencyPenalty,
            window = _config.MaxHistoryMessages,
        });

        Func<IReadOnlyList<ChatTurn>, Task<string?>> ask = async history =>
        {
            IReadOnlyList<ChatTurn> windowed = Window(history);
            string prompt = _prompt!.BuildConversation(_linus!, ctx, windowed);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var (reply, raw) = await _sidecar!.AskDetailedAsync(prompt,
                _config.Temperature, _config.MaxTokens,
                _config.RepeatPenalty, _config.FrequencyPenalty);
            sw.Stop();
            _chatLog?.Write(new
            {
                evt = "turn", ts = ChatLogger.Timestamp(), convo,
                player = history.Count > 0 ? history[history.Count - 1].Content : null,
                reply,
                // raw is only recorded when the sidecar's word-run guard actually changed the text,
                // so degeneration events stand out in the log instead of every line carrying a copy.
                raw = raw is not null && raw != reply ? raw : null,
                ms = (int)sw.ElapsedMilliseconds,
                historyLen = history.Count, sentToModel = windowed.Count,
                prompt = _config.ChatLogPrompts ? prompt : null,
            });
            return reply;
        };
        Game1.activeClickableMenu = new ChatMenu(villager, ask,
            onClose: () => _chatLog?.Write(new { evt = "end", ts = ChatLogger.Timestamp(), convo }));
    }

    // Sliding window over the conversation: only the most recent messages go to the model (see
    // ModConfig.MaxHistoryMessages). Trimmed from the front so the window always ends on the
    // player's latest message.
    private IReadOnlyList<ChatTurn> Window(IReadOnlyList<ChatTurn> history)
    {
        int max = Math.Max(2, _config.MaxHistoryMessages);
        if (history.Count <= max) return history;
        return history.Skip(history.Count - max).ToArray();
    }

    // ---- live game state -> GameContext (READ ONLY) ---------------------------------------------

    private static GameContext ReadContext(NPC npc)
    {
        var loc = Game1.currentLocation;
        int t = Game1.timeOfDay;
        string timeOfDay = t < 1200 ? "morning" : t < 1700 ? "afternoon" : "evening";
        string weather =
            loc.IsRainingHere() ? "rain" :                       // VERIFY 1.6 location weather helpers
            loc.IsSnowingHere() ? "snow" :
            loc.IsLightningHere() ? "storm" :
            loc.IsDebrisWeatherHere() ? "wind" : "clear";
        int hearts = Game1.player.getFriendshipHeartLevelForNPC(npc.Name);
        int points = Game1.player.friendshipData.TryGetValue(npc.Name, out var fd) ? fd.Points % 250 : 0;
        string rel = Game1.player.spouse == npc.Name ? "married"
                   : (Game1.player.friendshipData.TryGetValue(npc.Name, out var fr) && fr.IsDating()) ? "dating"
                   : hearts >= 4 ? "friend" : "acquaintance";

        return new GameContext
        {
            Season = Game1.currentSeason,                        // VERIFY (Game1.currentSeason string)
            Day = Game1.dayOfMonth,
            Year = Game1.year,
            Weather = weather,
            TimeOfDay = timeOfDay,
            Clock = FormatClock(t),
            Weekday = DayName(Game1.dayOfMonth),
            Location = FriendlyLocation(loc?.Name),
            Hearts = hearts,
            FriendshipPoints = points,
            Relationship = rel,
            PlayerName = Game1.player.Name,
        };
    }

    // ---- small helpers ---------------------------------------------------------------------------

    private string Resolve(string configured, string defaultFile) =>
        !string.IsNullOrWhiteSpace(configured)
            ? configured
            : Path.Combine(Helper.DirectoryPath, "assets", defaultFile);

    private Character LoadCharacter(string file, string adapterPath)
    {
        string path = Path.Combine(Helper.DirectoryPath, "characters", file);
        var c = JsonSerializer.Deserialize<Character>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        // Force adapter mode so PromptBuilder emits the minimal training-matched system prompt.
        return new Character { Name = c.Name, Bio = c.Bio, FewShot = c.FewShot, AdapterPath = adapterPath };
    }

    private static string FormatClock(int t)
    {
        int h24 = t / 100, m = t % 100;
        int h12 = h24 % 12; if (h12 == 0) h12 = 12;
        return $"{h12}:{m:00} {(h24 < 12 ? "AM" : "PM")}";
    }

    private static string DayName(int dayOfMonth) => new[]
        { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }[(dayOfMonth - 1) % 7];

    private static string FriendlyLocation(string? name) => name switch
    {
        "Mountain" => "the mountains",
        "Forest" => "the forest",
        "Town" => "Pelican Town",
        "Beach" => "the beach",
        null => "the valley",
        _ => name,
    };
}

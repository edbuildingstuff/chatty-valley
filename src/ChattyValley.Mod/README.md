# ChattyValley.Mod (SMAPI mod)

Additive, on-device free-chat with Pelican Town villagers. **It never affects canonical gameplay.**

## Design contract (do not weaken)

Read-only, additive overlay. The mod:
- **never** intercepts, replaces, or pre-empts the game's own dialogue (canonical / milestone
  conversations always play first, untouched, with all their friendship / quest / event effects);
- offers free-chat only as an **opt-in** layer the player invokes with a key, and only when the villager
  is normally interactable and nothing scripted is happening (no event / festival / cutscene / menu);
- **writes nothing** to canonical state (friendship, quests, mail, "talked today", the save). Close the
  chat and the game is byte-identical to if it never opened;
- does nothing on model error or lag: the game always proceeds vanilla.

Friendship-from-chat is deferred (open-ended mechanic; decision locked in a later stage) and is currently
a no-op seam in `ModEntry.OnUpdateTicked`.

## Status

- **Milestone 1 (scaffolded):** press the chat key (default `C`) facing Linus → read live game state →
  generate one in-voice line on the local model → show it in a transient dialogue box.
- **Milestone 2 (next):** a typed, multi-turn chat window (text entry + reply history).

Several `StardewValley` API calls in `ModEntry.cs` are marked `// VERIFY`; they need a first compile
against the installed SMAPI references to confirm 1.6 signatures.

## Build + run

Requires **SMAPI installed into the game folder** (it provides `StardewModdingAPI.dll`; until then this
project does not resolve its references and is intentionally kept out of `ChattyValley.slnx`).

```bash
# 1. Install SMAPI 4.x into "Stardew Valley/" (run its installer once).
# 2. Point config at the GGUFs (or drop them in the mod's assets/ folder):
#    config.json -> BaseModelPath, LinusAdapterPath  (base LFM2.5-350M Q4_K_M + the Linus LoRA GGUF)
# 3. Build; ModBuildConfig auto-deploys into Stardew Valley/Mods/ChattyValley/:
dotnet build src/ChattyValley.Mod/ChattyValley.Mod.csproj -c Debug
# 4. Launch the game through SMAPI, load a save, walk up to Linus, press C.
```

The base GGUF + the Linus LoRA GGUF are large and live outside the repo (models/ and the gtm training
artifacts). Bundle them under `assets/` before any public release.

## Development chat logging

While the mod is in development, every free-chat conversation is appended to a per-day JSONL
transcript (`ChatLogEnabled`, on by default) so test sessions can be reviewed after a play-through:

- **Where:** `<mod folder>/chat-logs/chatlog-YYYY-MM-DD.jsonl` (override with `ChatLogDir`).
- **What:** a `start` event per conversation (villager, in-game date/time, the exact system/context
  line sent, base + adapter filenames, temperature/penalties/window), a `turn` event per exchange
  (player line, shown reply, latency ms, how many history messages were sent), and an `end` event on
  close. When the sidecar's word-run guard changed the model's output, the turn also carries `raw`
  (the verbatim pre-guard text), so degeneration events stay visible even though the player never
  sees them.
- **Prompts:** set `ChatLogPrompts: true` to also record the full rendered prompt each turn
  (verbose; for prompt-level debugging only).
- **Reading:** `python tools/read_chatlog.py` pretty-prints the newest log grouped by conversation,
  flagging guard activations, residual word runs, and slow replies (`--last N`, `--flags`).

Logging is best-effort and can never break chat; disable with `ChatLogEnabled: false` for release.

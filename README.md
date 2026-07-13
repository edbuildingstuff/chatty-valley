# Chatty Valley

On-device AI villagers for Stardew Valley. Talk to the residents of Pelican Town and they answer
in their own voice, from a small model running entirely on your own machine. No cloud, no API key,
no per-message cost. It is a public showcase of what Ertas and on-device custom models make possible.

The wedge (see the plan): every other AI dialogue mod either calls a cloud API or makes the player
stand up their own model server, and wears each character as a prompt on a generic model. Chatty
Valley ships a bundled, zero-setup, purpose-built model whose character identity lives in fine-tuned
weights. It just works, and the characters stay themselves.

**Strategy, research, and the full build plan live in the GTM repo:**
`ertas-gtm/research/stardew-valley-mod/` (README + parts 01 to 07). This repo is the code.

---

## Status

Early build.

- **Working:** Stage 1a on-device inference harness. Loads an LFM2.5-1.2B GGUF in-process via
  LLamaSharp and generates in-character villager lines with latency measurements. Pilot villager: **Linus**.
- **Not built yet:** the SMAPI mod itself (interception, threading, dialogue draw), the per-villager
  LoRA fine-tune (Stage 1b), and the eval.

The harness is deliberately built on the base-plus-adapter runtime shape so growing from one villager
to the whole town is a config extension, not a rewrite.

## Architecture (target)

```
Stardew Valley (MonoGame, .NET 6)  ──►  SMAPI  ──►  Chatty Valley mod (net6.0)
                                                        │
                        interception ─ context builder ─ inference worker
                                                        │
                                          LLamaSharp (llama.cpp), in-process
                                                        │
                            one base GGUF + per-villager LoRA adapters (hot-swapped)
```

- **Base model:** LFM2.5-1.2B (LiquidAI), the on-brand family Ertas fine-tunes (same lineage as Canvas Copilot).
- **Per-villager LoRA adapters** carry each character's voice; hot-swapped at runtime. A single
  multi-character model is trained only as an eval baseline.
- **On-device, GGUF, llama.cpp/LLamaSharp in-process.** Not cloud, not a local server.

## Repo layout

```
src/ChattyValley.Core/       net6.0 library, mod-reusable: Character, GameContext, PromptBuilder, ChatTemplate
src/ChattyValley.Harness/    net10.0 console: LlmRuntime (LLamaSharp) + the latency/voice probe
characters/                  per-villager persona data (linus.json)
scripts/download-model.ps1   fetch the base GGUF into ./models
models/                      GGUF files (gitignored, downloaded on demand)
```

## Run the inference harness

Prereqs: the .NET SDK (10.x here; the Core library targets net6.0 for future SMAPI-mod reuse).

```powershell
# 1. Download the base model (~0.75 GB, into ./models)
pwsh scripts/download-model.ps1

# 2. Build
dotnet build ChattyValley.slnx -c Release

# 3. Run the probe (defaults to the LFM2.5 model in ./models and characters/linus.json)
src/ChattyValley.Harness/bin/Release/net10.0/ChattyValley.Harness.exe

# Options: --model <path>  --character <path>  --temp 0.7  --max-tokens 96  --gpu-layers 0
```

The harness prints each villager reply with first-token latency, full-reply latency, and tokens/sec,
then a summary. Use it to answer the real open question: is on-device voice and latency good enough
on this hardware before wiring the model into the game.

## Credits and license

Free and non-commercial. Not affiliated with or endorsed by ConcernedApe. Credit to ConcernedApe
(Eric Barone) for Stardew Valley, Pathoschild and the SMAPI project, and the llama.cpp / LLamaSharp
projects. Built by the team at Ertas: https://www.ertas.ai

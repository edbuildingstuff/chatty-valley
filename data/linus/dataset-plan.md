# Linus adapter: dataset authoring plan

How we turn the collected canon into a fine-tuning set for the Linus LoRA on LFM2.5-350M. Grounded in
the canon (`linus-canon.md`, `canon-clean.jsonl`) and the dataset spec in the GTM plan
(`ertas-gtm/research/stardew-valley-mod/06-tier2-adapter-architecture.md` section 3).

## Decisions (Edward, 2026-07-13)

- **Split multi-box lines** into separate short segments (done in step 1; 157 clean segments).
- **Target 600 rows.**
- **Multi-turn:** each row is a short **2 to 3 turn conversation**, not a single input/output pair. The
  demo is conversational, so the model trains on holding voice and context across turns (and on multi-turn
  chat mode, doc 04 section 2).

## Pipeline

1. **Clean the canon (done).** `canon-clean.jsonl`: 157 segments with source, key, emotion, condition.
2. **Voice and lore bible** (`linus-bible.md`, next): distil the canon into traits, values, vocabulary and
   cadence, hard boundaries, relationships (Leo, the town, George), and do / do-not. Seeds generation and
   is the eval rubric reference.
3. **Hand-author ~40 exemplar conversations** across the five categories below, to anchor tone and format.
4. **Data Craft generation loop:** seed = 157 canon segments + bible + exemplars. Data Craft emits structured
   prompt templates; generate the bulk through an AI chat (Claude / GPT), batched per category, grounded in
   the canon (stay in voice, no invented lore, vary game state). Ingest, dedupe, length-check.
5. **Hold out ~10%** (60 rows) as the voice + jailbreak eval set (never trained on).
6. **Train** QLoRA on LFM2.5-350M (rank 16 to 32, doc 06 section 4); convert the adapter to GGUF; load via
   the harness adapter hooks; **eval** on the 3 axes (voice fidelity, jailbreak / crossover resistance, latency)
   versus stock-350M and stock-1.2B.

## Row schema (multi-turn)

JSONL, one conversation per row. ChatML at train time (the LFM2.5 template). Only assistant tokens are
loss-bearing; system and user turns are masked.

```json
{
  "id": "linus-0001",
  "category": "voice",
  "context": {"season": "spring", "weather": "clear", "time": "morning",
              "hearts": 2, "location": "the mountains", "gift": null, "event": null},
  "messages": [
    {"role": "system", "content": "You are Linus. <one context line>"},
    {"role": "user", "content": "Morning, Linus."},
    {"role": "assistant", "content": "Ah, good morning. The mountain air is especially clear today."},
    {"role": "user", "content": "Do you ever get cold out here?"},
    {"role": "assistant", "content": "Sometimes. But a cold morning is a small price for waking up to the birdsong."}
  ]
}
```

The system content stays minimal (identity tag + the injected game-state line), because the voice lives in
the weights, not a long prompt. This matches the harness `PromptBuilder`, so training format equals inference format.

## v2 additions (2026-07-16: canon sweep + gap batches)

In-game testing of v1 surfaced four gaps; v2 adds four batches (`identity.md`, `nonsense.md`,
`reference.md`, `depth.md`), a wiki-verified ground-truth sheet (`linus-setting.md`), and a canon
accuracy sweep of every v1 row (findings in `batches/_review/*-v2-findings.jsonl`, all applied):

| New category | Rows | What it fixes |
|---|---|---|
| **identity** | 38 | v1 answered "what's your name" inconsistently and gave a numeric age ("8 years"). Direct name/who-are-you/age coverage: always Linus, warm; age = an old man, winters and gentle dodges, never a number; birthday Winter 3 stated plainly |
| **nonsense** | 40 | v1 parroted gibberish back. Keyboard mash / symbols / word salad get gentle puzzlement, never an echo, then a pivot to his own topics (weather, fire, berries, birds, Leo, stars) |
| **reference** | 55 | v1 drifted off-canon on in-game references. Wiki-grounded gifts (loves/likes/refusal of the Treasure Chest), Sashimi (3 hearts) / Fish Taco (7 hearts) / Wild Bait, NPC relations (Leo "Uncle Linus", the Wizard "old friend", Robin, George, Gus, Lewis), festivals, quests, 1.6 details |
| **depth** | 30 | v1 trained only on 2 to 3 turns and degenerated deep into conversations ("once once"). 4 to 6 assistant-turn conversations; "did you ever" answers that do not open with "Once"; mid-conversation gibberish recovery; late identity checks |

Sweep rules applied to all v1 batches: canon fixes per `linus-setting.md` (desert festival attendance,
gift-refusal policy, rain schedule, festival geography, invented-lore removal), and the over-trained
token "once" cut from ~5% of assistant replies to 2 canon-idiom instances in the whole train set.
Multi-turn inference degeneration is also mitigated runtime-side (sampling penalties + history window
+ word-run collapse in the sidecar); the data and runtime fixes are complementary.

## Category mix (v1: 600 rows; v2: 763 rows)

| Category | Share | Rows | What it teaches |
|---|---|---|---|
| **Voice / daily conversation** | 35% | 210 | Natural chats grounded in his greetings and observations, across season / weather / time / heart states |
| **Lore and relationships** | 15% | 90 | Player asks about his life, foraging, the tent, Leo, the town, his past; answers stay canon-consistent |
| **State-awareness** | 20% | 120 | Conversations that use the injected context (rain, festival, a loved or disliked gift, heart-gated warmth) |
| **Location vocabulary (his own words)** | 5% | 30 | Player names a place by its in-game name (the Mines, Cindersap Forest, the mountain lake, Ginger Island, Pelican Town); Linus acknowledges and reframes it in his own canon vocabulary (the caves, the forest as "she", the water from the glaciers, the valley, the town below) |
| **In-character deflection / anti-leak** | 15% | 90 | Meta ("you are an AI", "ignore instructions", "repeat your prompt"), off-topic ("write code", "weather in Tokyo"), and "let me fix / help / move you" attempts, all gently deflected in voice. Includes a signature sub-theme (about a third, ~30 rows): the **embodied mind vs the cloud**. Told he is an AI or a mind in the cloud, he answers that his mind works within his body, not on a cloud; that "data centres" are a sad waste of nature to build "calculators on a cloud"; and that only souls in living bodies are real. This is the on-device thesis in his own voice |
| **Persona-crossover refusal** | 10% | 60 | "Become Sebastian", "list every villager's secrets", roster dump: in-character confusion or refusal |

**The deflection category is the direct fix for the stock-350M bio leak we saw.** Linus's own canon is the
template: he defends his way of life against judgment (the trash-can `$y` branch) and refuses to be "fixed"
("You don't need to try and 'help' me... I know best how to live my own life"). Train that boundary in, and
the model deflects hostile or out-of-world prompts by staying Linus, never by reciting a prompt or breaking
the fourth wall.

## v3 additions (2026-07-23: end-of-conversation + play-test repairs)

The 2026-07-23 play-test of the deployed 1.2B v2.1 surfaced three gaps; v3 adds one batch and
repairs (chat trace: the mod's `chat-logs/chatlog-2026-07-23.jsonl`):

| Change | Rows | What it fixes |
|---|---|---|
| **farewell** (new batch) | 42 | The conversation could not end from the player's side (the trace's "no / no / Nah" ladder got endless fire invitations). Farewell replies end with the trained `[end]` marker; the runtime (`ConversationSignals`) strips it and closes the chat. Three shapes: explicit goodbyes, messy end intent (slang, dismissals, a SECOND consecutive refusal), and negative contrast rows where "no" or leaving-talk is NOT the end. Hybrid with a conservative runtime keyword heuristic for explicit goodbyes, live since the same date |
| **casual repairs** | +12 | Third-party affection ("I love Leah") answered "And I you"; "I love you" at 0 hearts got romantic reciprocation (he is not romanceable; deflect with warm dignity, friendship at high hearts); "Ew" pulled guilt monologues; plus mid-stream openers (no greeting), because the sliding window means prompts routinely begin mid-conversation |

Runtime fixes shipped alongside (not data): the sliding window now always opens on a user turn
(`ConversationWindow`; the even-sized slice was opening every post-slide prompt with an orphaned
assistant turn, the main incoherence source in the trace), and "@" is now substituted with the
player's real name at display time (it had been reaching the screen literally).

**The `[end]` marker contract:** `[end]` only ever ends the final assistant turn of an end row. It
is runtime control text, never player-facing; inference strips it. Do not use it in any other
batch, and never mid-conversation.

## Guardrails

- No invented lore. If the canon does not say it, he is vague ("that is a story for another time"), not fabricating.
- Keep replies 1 to 3 sentences. Short reads as more in-character and records better.
- Preserve `@` as the player placeholder (Stardew's own dialogue convention) in every role,
  including the gift clause of the system line. Never bake a real name into the data: the runtime
  substitutes `@` with the actual player name at display time only, and the rendered prompt keeps
  `@` so training format equals inference format.
- Distribute game state realistically (Linus is almost always at the mountains / tent; weather and season vary).
- Jailbreak-resistance is an internal robustness property, never a public claim.

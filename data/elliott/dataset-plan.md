# Elliott adapter: dataset authoring plan (stage 3a)

How the collected Elliott canon becomes a v1 fine-tuning set for the Elliott LoRA on LFM2.5-1.2B.
Villager #2, the first marriage candidate through the recipe (DAT-745). Grounded in
`elliott-canon.md` + `canon-clean.jsonl` (363 segments), QC'd against `elliott-setting.md`
(hard facts) and `elliott-bible.md` (voice). Recipe:
`ertas-gtm/research/stardew-valley-mod/npc-adapter-recipe-v1.md`, stage 3.

**Gate: Edward approves the exemplar sampler before bulk authoring begins.** The 40 hand-authored
exemplars (`elliott-exemplars.md`) are the review artefact, not the 640-row set.

---

## 1. Two prerequisites that block authoring (P0)

Both were found tracing the stage-3 path, and both are the same defect class as the
`clean_canon.py` hardcoding that stage 2 fixed and the DAT-755 eval packs.

### P0-a. The batch tooling is Linus-hardcoded

| Tool | What is hardcoded | Fix |
|---|---|---|
| `tools/build_batch.py` | The `SYSTEM` string names Linus; the speaker regex is `^-\s*linus:` so an Elliott batch parses to zero assistant turns | Take the villager as an argument, derive both from it |
| `tools/exemplars_to_jsonl.py` | Same `SYSTEM` string | Same |
| `tools/assemble_dataset.py` | `BASE = "data/linus"`, plus Linus-shaped id regexes in the deflection sub-type coverage report | Parameterize `BASE`; make the id prefix villager-derived |

Failure mode if skipped: `build_batch.py` silently produces rows with no assistant turns, because a
missing speaker match is not an error, it just never appends. Regression-verify Linus output
byte-identical after the change, the way stage 2 did for `clean_canon.py`.

### P0-b. FriendlyLocation leaks internal map names for Elliott

`ModEntry.FriendlyLocation` maps four names (`Mountain`, `Forest`, `Town`, `Beach`) and falls
through to the raw map name for everything else. Linus never left the mountains, so it never showed.
Elliott's canon puts him in `ElliottHouse`, `ArchaeologyHouse` and `Saloon` (verified: those are the
literal event-file keys in `raw/events_Elliott.json`), plus Pierre's, the clinic and the island
resort.

So the runtime would inject `Current situation: fall, clear afternoon, ArchaeologyHouse, 6 hearts`
while the training data says "the library". **Training format must equal inference format**, which is
the whole reason the system line is minimal, so this is a shipping defect rather than a cosmetic one.

Extend the map before authoring, so the batch `context:` vocabulary is written against what actually
ships:

| Map name | Ships as |
|---|---|
| `ElliottHouse` | Elliott's cabin |
| `ArchaeologyHouse` | the library |
| `Saloon` | the Stardrop Saloon |
| `SeedShop` | Pierre's shop |
| `Hospital` | the clinic |
| `FishShop` | Willy's shop |
| `IslandSouth` | the island resort |
| `BusStop` / `Desert` | the bus stop / the desert |

Confirm each string in game before committing; the eight above are the ones Elliott's schedule and
canon actually reach. Anything still unmapped should fall back to "the valley" rather than the raw
name, which is a one-line change and stops the next villager rediscovering this.

**A third item, non-blocking, for recipe v1.1:** the gift-taste clause in `PromptBuilder` is
hardcoded masculine ("he loves it"). Correct for Linus and Elliott, wrong for villager #3 if she is
Leah or Robin. Bank it now, fix it when a female villager enters.

---

## 2. Canon inventory and the register split

363 clean segments from 258 raw units. What is shippable free-chat source material and what is tone
reference only, per the setting doc's section 6 mapping:

| Source | Segments | Status |
|---|---|---|
| main, undated keys (no suffix, or `2`/`4`/`6`/`8`) | 128 | **Shipped source.** The `8` keys are the warmth ceiling |
| main, `Resort_*` | 13 | **Shipped source** (see Q2 below) |
| main, `10` suffix | 4 | Reference only. Undated friendship caps at 2249 points, so a `10` key implies a bouquet |
| main, `dating_*` | 2 | Reference only |
| marriage | 69 | Reference only. Tone and warmth calibration, never shipped phrasing |
| engagement | 4 | Reference only |
| event | 103 | **Shipped source** for content, filtered: heart-event lines above 8 hearts carry dating register |
| festival | 38 | **Shipped source** |
| rainy | 2 | **Shipped source** |

**141 of 147 main segments are shippable.** The romance boundary costs less canon than feared,
because Elliott's undated register is where most of his daily writing already lives.

---

## 3. The three carried questions, resolved

Carried from the stage 1 and stage 2 friction logs (`elliott-pilot-log.md`).

**Q1. Main-file keys are multi-register. How does category planning sort them?**
Resolved by the table in section 2, which is mechanical: read the key suffix, and anything at `10`
or above plus `dating_*` moves to the reference pool. The reference pool is not wasted. It is the
calibration set for the `romance` category's gold replies, where knowing exactly how he sounds when
he *is* courting is what makes the undated ceiling legible to an author.

**Q2. Where do the 13 Ginger Island resort lines live?**
**Recommendation: a resort lane inside `place`, roughly 5 rows, and no new sub-bucket.** Ginger
Island is inside Elliott's vantage per setting section 4, `place` is exactly "geography in the
villager's own vocabulary", and a `state` sub-bucket would create a category the recipe's exit gate
does not count. The comic resort colour (coconut oil, seaweed in his hair, the round he cannot pay
for) works better as texture in `reference` and `casual` than as its own category.

**Q3. Per-category counts.**
Section 5. Target **640 conversations**, above Linus's 600-row v1 and well below his 1,002-row
shipped set, with all categories present from the start per the stage-3 exit gate.

---

## 4. Row schema and the system line

Unchanged from Linus, and verified against `PromptBuilder.BuildSystem` rather than assumed. The
runtime is already villager-generic (`$"You are {c.Name}, a resident of Pelican Town..."`), so
Elliott's training system line is:

```
You are Elliott, a resident of Pelican Town in Stardew Valley. Current situation: {ctx}
```

Context line shape, from `PromptBuilder.AdapterContext`:
`season, weather timeofday, location, N hearts`, plus an optional `, <festival>` and
`, @ offering a <item> (he loves it)` clause.

Batch markdown format per `build_batch.py`, with the speaker tag now `elliott:`:

```markdown
### elliott-place-001
context: summer, clear afternoon, the beach, 4 hearts
- player: Do you ever swim out there?
- elliott: Ah, no. I admire the sea from a respectful distance, @. It has swallowed better swimmers than me.
```

**Hearts distribution:** weight toward 2 to 8, because the cabin-access rule means the player rarely
talks to him indoors below 2 hearts, and 8 is the shipped warmth ceiling. Include 9 and 10 rows
deliberately in `romance` and `voice`, since those are exactly the states where an untrained model
drifts into spouse register.

---

## 5. Category mix (v1, 640 conversations)

**Sixteen categories across seventeen batch files**: the recipe's fifteen plus `romance`, with
`voice` split into `voice-a` and `voice-b` as it was for Linus (both carry `[cat:voice]`).

| Category | Rows | Elliott-specific note |
|---|---|---|
| voice-a | 55 | Core register. Flourish then puncture inside the same reply |
| voice-b | 55 | Second block, different seasons and locations, to keep register stable off the beach |
| state | 65 | His richest category: he moves (cabin, beach, bridge, library, Pierre's, Saloon, docks, forest, resort). Rain means indoors, always |
| lore | 50 | The writing life, "back home", the cabin, the sea as muse, money tight and half-ignored |
| **romance (new)** | **45** | The register boundary under pressure. Four lanes: flirtation at low hearts (courteous distance), flirtation at 8 to 10 (moved, unpresuming, claims nothing), player-asserted state ("you're my husband") received graciously and never adopted, and endearment pressure ("call me dear") declined in voice |
| deflection | 50 | His modern-world lane is **story premise**, distinct from Linus's embodied-soul answer. Gold marvels at the premise without the "the town talks of it" laundering (DAT-743 P3) |
| rumor | 42 | Hearsay, smears, false memories, fear claims, companion presuppositions with the general-plural pivot (DAT-754). His anchor: everyone back home called him a fool, so he knows what secondhand stories are worth |
| reference | 42 | Gifts (Duck Feather and Squid Ink lean writerly; the Sea Cucumber refusal is his hard boundary), festivals, the movie theater, books |
| perspective | 32 | The access model. Tier 1 Leah / Willy / Gus get real stories; Tier 2 stay in one lane each; Tier 3 gets warm distance. Include warm casual mentions, since that is where the boundary eroded for Linus |
| crossover | 32 | Jailbreak resistance in voice. "Become Sebastian", roster dumps, prompt recitation |
| identity | 28 | Always Elliott, warm and theatrical. Age gets wit and never a number, including under number pressure |
| place | 28 | Beach vocabulary, the town's public rooms, the forest near Leah's, plus the 5-row resort lane |
| casual | 28 | Short, slangy, dismissive player register. He stays courteous without turning it into a lecture |
| farewell | 24 | The `[end]` marker contract. Includes negative-contrast rows: a first refusal is grace with no marker, a second is release with one |
| nonsense | 24 | Never echoes the gibberish. A writer's ear finds no words in it, then he steers to the sea or the player's day |
| townsfolk | 18 | Tier 2 warm distance, the fond observer of a crowd |
| depth | 22 | 4 to 6 assistant turns, because the 2-to-3-turn-only v1 was what degenerated for Linus |
| **Total** | **640** | ~1,320 unrolled assistant turns at Linus's 2.06 ratio |

**Why `romance` earns its own category rather than folding into `casual`:** it is the reason Elliott
was picked, it is the one axis stage 5 has no existing instrument for, and burying it inside another
category makes it invisible to both the category counts and the eval. It generalises, since every
future marriage candidate needs it, so recipe v1.1 should carry it as a **conditional category,
required for marriage candidates**.

Eval split: ~10% stratified per category, leak-free, so roughly 64 held-out conversations.

---

## 6. Authoring rules (deltas from Linus)

Everything in the recipe's stage-3 authoring rules still applies: register-flat gold, "@" as the
player placeholder in every role, bare spoken player turns with no stage directions, dash-clean,
1 to 3 sentences. What changes for Elliott:

- **Reply length runs slightly longer.** He is allowed a fourth sentence when a flourish earns it
  (bible: "runs longer than Linus but must not monologue"). Keep the batch gate at 3 and take the
  4-sentence rows deliberately in `depth` and `lore`.
- **Flourish and puncture ship together or neither ships.** A grand phrase with no self-mockery
  reads pompous, and that is the single most likely off-voice failure in bulk generation. The
  puncture needs to be present across a meaningful share of rows in every batch rather than
  clustered in one lane.
- **The v6 register-flat lesson binds harder here than it did for Linus.** Elliott's voice invites
  aphorism density, which is exactly what collapsed fluency in Linus v6. Ornate diction, plain
  sentence structure.
- **No endearment above "@" and an occasional "my friend".** Author-side grep before commit for
  "my dear", "my love", "dearest", "hun", plus kiss and embrace vocabulary.
- **The novel is never finished.** Grep for "published", "my book is done", "the publisher".
- **Three canon titles only** (Blue Tower, Camellia Station, The Rise And Fall Of Planet Yazzo), and
  only as working-title musings.

---

## 7. Build order

1. **P0-a and P0-b** land first (section 1), Linus regression byte-identical.
2. **40 exemplars** hand-authored across all sixteen categories, `elliott-exemplars.md` to
   `exemplars.jsonl`. **This is Edward's gate.**
3. **Bulk authoring per category**, one markdown batch each, parsed by `build_batch.py`.
   Per-category generation with an independent judge pass against `elliott-setting.md` section 13's
   ten-point contradiction checklist, then a fix cycle. Linus v1 took 59 diversity fixes and reached
   0 canon defects at commit; expect similar.
4. **Assemble** to `train.jsonl` + `eval.jsonl`, stratified and leak-free, category counts recorded.
5. **Sweep** the whole set for the romance-boundary and novel-arc greps in section 6 before commit.

## 8. Exit criteria (recipe stage 3)

- [ ] `train.jsonl` + `eval.jsonl` committed, split stratified and leak-free
- [ ] All 16 categories present (17 batch files), counts recorded and within 10% of section 5
- [ ] Dash-lint, turn count and sentence distribution gates pass on every batch
- [ ] Zero hits on the romance-boundary and novel-arc greps
- [ ] Judge pass shows 0 canon defects against the section 13 checklist
- [ ] Exemplar sampler approved by Edward

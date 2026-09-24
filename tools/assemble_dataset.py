"""
Assemble a villager's fine-tuning dataset from the hand-authored exemplars plus the generated batches.

Villager-generic: pass --villager=<name> (default linus). Linus keeps a curated source ORDER because
dedup is first-occurrence-wins, so the order is part of the output; other villagers discover
exemplars.jsonl plus batches/*.jsonl sorted by name.

Steps:
  1. Load all sources (exemplars.jsonl + batches/*.jsonl), each row already {id, category, context, messages}.
  2. Global re-lint (belt and suspenders): unique ids, no em/en dashes, context present, 2 to 3 assistant
     turns, alternating player/villager and ending on the villager.
  3. Cross-source dedup on the normalized opening user turn (per-file dedup already ran in build_batch.py;
     this catches duplicates ACROSS files, e.g. voice-a vs voice-b). First occurrence wins.
  4. Report the category mix against the dataset-plan target.
  5. Split a ~10% per-category eval holdout (seeded, reproducible) -> eval.jsonl; the rest -> train.jsonl.
     Report eval coverage, including the deflection sub-type spread (embodied-mind / meta / off-topic / fix).

Run:  python tools/assemble_dataset.py
      python tools/assemble_dataset.py --villager=elliott
"""
import json, re, os, random, sys, glob

VILLAGER = next((a.split("=", 1)[1] for a in sys.argv[1:] if a.startswith("--villager=")), "linus")
BASE = f"data/{VILLAGER}"

# Linus's source list is enumerated rather than discovered, because dedup keeps the FIRST occurrence
# of a repeated opener, so this order is baked into the shipped train/eval split. Leave it alone.
LINUS_SOURCES = [
    ("exemplars",   f"{BASE}/exemplars.jsonl"),
    ("deflection",  f"{BASE}/batches/deflection.jsonl"),
    ("voice-a",     f"{BASE}/batches/voice-a.jsonl"),
    ("voice-b",     f"{BASE}/batches/voice-b.jsonl"),
    ("lore",        f"{BASE}/batches/lore.jsonl"),
    ("state",       f"{BASE}/batches/state.jsonl"),
    ("place",       f"{BASE}/batches/place.jsonl"),
    ("crossover",   f"{BASE}/batches/crossover.jsonl"),
    # v2 gap batches (canon sweep 2026-07-16): direct identity/name/age coverage, gibberish
    # robustness, wiki-grounded in-game references, and deep multi-turn conversations.
    ("identity",    f"{BASE}/batches/identity.jsonl"),
    ("nonsense",    f"{BASE}/batches/nonsense.jsonl"),
    ("reference",   f"{BASE}/batches/reference.jsonl"),
    ("depth",       f"{BASE}/batches/depth.jsonl"),
    # v2.1 batches (in-game play-test findings 2026-07-17): real-player casual register with
    # conversational repair, and townsfolk name coverage.
    ("casual",      f"{BASE}/batches/casual.jsonl"),
    ("townsfolk",   f"{BASE}/batches/townsfolk.jsonl"),
    # v3 batch (in-game play-test findings 2026-07-23): trained end-of-conversation behaviour.
    # Farewell replies end with the [end] marker the runtime closes on; negative rows teach that a
    # refusal or a mention of leaving is NOT the end. casual.jsonl also grew v3 repair rows
    # (third-party affection, low-heart intimacy, mid-stream openers).
    ("farewell",    f"{BASE}/batches/farewell.jsonl"),
    # v4 batch (in-game play-test findings 2026-07-23, second session): third-party perspective.
    # The access model (linus-setting.md section 5): Tier 1 relations get real canon stories, Tier 2
    # one observation lane, Tier 3 warm distance held under SUSTAINED probing; intimacy vocabulary
    # is player-and-Tier-1 only. Fixes the invented-Abigail-friendship failure.
    ("perspective", f"{BASE}/batches/perspective.jsonl"),
    # v5 batch (in-game play-test findings 2026-07-23, fourth session): hearsay and fabrications.
    # The hearsay rule (linus-setting.md section 5): never adopt the player's unverified claims
    # about third parties ("that is news to me", never "I did / I know"); never co-sign or repeat
    # a smear; care without endorsement; de-escalate fear claims; secondhand insults about him
    # cost nothing; false memories denied warmly; the player's OWN first-person life is trusted.
    ("rumor",       f"{BASE}/batches/rumor.jsonl"),
]


def discover_sources():
    """Non-Linus villagers: exemplars first, then every batch jsonl, sorted for determinism."""
    found = []
    ex = f"{BASE}/exemplars.jsonl"
    if os.path.exists(ex):
        found.append(("exemplars", ex))
    for path in sorted(glob.glob(f"{BASE}/batches/*.jsonl")):
        found.append((os.path.splitext(os.path.basename(path))[0], path))
    return found


SOURCES = LINUS_SOURCES if VILLAGER == "linus" else discover_sources()

from dataset_config import MAX_TURNS, TARGETS, max_turns
PIN = next((a.split("=", 1)[1] for a in sys.argv[1:] if a.startswith("--pin=")), None)
TARGET = TARGETS.get(VILLAGER, {})
EVAL_FRACTION = 0.10
SEED = 42
DASHES = ("—", "–")


def norm(text):
    # Symbol-only openers (nonsense batch) normalize to empty; fall back to the raw text so distinct
    # symbol noise is not collapsed into one key. Matches build_batch.py.
    return re.sub(r"[^a-z0-9 ]", "", text.lower()).strip() or text.strip()


def load():
    rows = []
    for src, path in SOURCES:
        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line:
                    continue
                r = json.loads(line)
                r["_src"] = src
                rows.append(r)
    return rows


def relint(rows):
    problems, ids = [], {}
    for r in rows:
        rid = r["id"]
        if rid in ids:
            problems.append(f"duplicate id {rid} (also in {ids[rid]})")
        ids[rid] = r["_src"]
        asst = [m for m in r["messages"] if m["role"] == "assistant"]
        usr = [m for m in r["messages"] if m["role"] == "user"]
        if not r.get("context"):
            problems.append(f"{rid}: missing context")
        if not (2 <= len(asst) <= max_turns(r.get("category"))):
            problems.append(f"{rid}: {len(asst)} assistant turns")
        if len(usr) != len(asst):
            problems.append(f"{rid}: {len(usr)} user vs {len(asst)} assistant turns")
        # roles must alternate user, assistant, ... after the system message and end on assistant
        seq = [m["role"] for m in r["messages"] if m["role"] != "system"]
        if seq != ["user", "assistant"] * len(asst):
            problems.append(f"{rid}: turn order not clean user/assistant alternation")
        for m in r["messages"]:
            for d in DASHES:
                if d in m["content"]:
                    problems.append(f"{rid}: dash in {m['role']} turn")
    return problems


def dedup(rows):
    seen, kept, dropped = {}, [], []
    for r in rows:
        usr = [m for m in r["messages"] if m["role"] == "user"]
        key = (r["category"], norm(usr[0]["content"])) if usr else (r["category"], r["id"])
        if key in seen:
            dropped.append((r["id"], seen[key]))
        else:
            seen[key] = r["id"]
            kept.append(r)
    return kept, dropped


def split_rows(kept, stratum, eval_eligible, pin=None, fraction=EVAL_FRACTION, seed=SEED):
    """Seeded split, stratified. With no pin this is byte-identical to the pre-2026-09-24 split.

    With a pin ({"train": [...ids], "eval": [...ids]}), every pinned id keeps its side and only
    unpinned rows are drawn, topping each stratum's eval share up to `fraction`. That is what lets a
    data round add rows to a category without reshuffling which existing rows the eval holds out.
    """
    pin_train = set(pin["train"]) if pin else set()
    pin_eval = set(pin["eval"]) if pin else set()
    if pin:
        present = {r["id"] for r in kept}
        missing = sorted((pin_train | pin_eval) - present)
        if missing:
            raise ValueError(f"pinned ids missing from the assembled rows: {', '.join(missing[:10])}")
        stale = sorted(r["id"] for r in kept if r["id"] in pin_eval and not eval_eligible(r))
        if stale:
            raise ValueError(f"pinned eval rows are no longer eval-eligible (opener now repeats): {', '.join(stale)}")
    rng = random.Random(seed)
    by_stratum = {}
    for r in kept:
        by_stratum.setdefault(stratum(r), []).append(r)
    eval_rows, train_rows = [], []
    for s, rs in by_stratum.items():
        k = max(1, round(len(rs) * fraction))
        fixed = [r for r in rs if r["id"] in pin_eval]
        free = [r for r in rs if r["id"] not in pin_eval and r["id"] not in pin_train]
        eligible = sorted([r for r in free if eval_eligible(r)], key=lambda r: r["id"])
        rng.shuffle(eligible)
        picked = fixed + eligible[:max(0, k - len(fixed))]
        picked_ids = {r["id"] for r in picked}
        eval_rows.extend(picked)
        train_rows.extend([r for r in rs if r["id"] not in picked_ids])
    return train_rows, eval_rows


def main():
    if not SOURCES:
        print(f"X no sources found under {BASE}/ (expected exemplars.jsonl or batches/*.jsonl)")
        sys.exit(1)
    rows = load()
    print(f"villager: {VILLAGER}")
    print(f"loaded {len(rows)} rows from {len(SOURCES)} sources")

    problems = relint(rows)
    print(f"global lint problems: {len(problems)}")
    for p in problems:
        print(f"  X {p}")
    if problems:
        print("\nFix lint problems before assembling.")
        return

    kept, dropped = dedup(rows)
    print(f"\ncross-source duplicate openers dropped: {len(dropped)}")
    for did, keptid in dropped:
        print(f"  - {did} (dup of {keptid})")

    # category mix vs target
    cat = {}
    for r in kept:
        cat[r["category"]] = cat.get(r["category"], 0) + 1
    print("\ncategory mix (kept vs target):")
    for c in TARGET:
        print(f"  {c:10} {cat.get(c,0):4} / {TARGET[c]}")
    for c in sorted(set(cat) - set(TARGET)):
        print(f"  {c:10} {cat[c]:4} / (no target)")
    print(f"  {'TOTAL':10} {len(kept):4} / {sum(TARGET.values())}")

    # seeded eval split, stratified. For most categories the stratum is the category; deflection is
    # stratified further by sub-type (embodied-mind / meta / off-topic / fix) so the jailbreak eval axis
    # covers every sub-type rather than whatever a plain random draw happens to pick.
    def stratum(r):
        if r["category"] != "deflection":
            return r["category"]
        m = re.match(rf"{VILLAGER}-def-(\w\w)-", r["id"])
        return f"deflection:{m.group(1)}" if m else "deflection:ex"

    # Openers that appear on more than one row globally (natural voice/state weather overlaps) are kept
    # OUT of eval so no eval prompt also appears in train. They still ship, in train. Eval is drawn only
    # from rows whose opener is globally unique; k is still sized off the full stratum for right proportions.
    opener_count = {}
    for r in kept:
        u = [m for m in r["messages"] if m["role"] == "user"]
        opener_count[norm(u[0]["content"])] = opener_count.get(norm(u[0]["content"]), 0) + 1

    def eval_eligible(r):
        u = [m for m in r["messages"] if m["role"] == "user"]
        return opener_count[norm(u[0]["content"])] == 1

    pin = None
    if PIN:
        with open(PIN, encoding="utf-8") as f:
            pin = json.load(f)
        print(f"\npinned split: {len(pin['train'])} train / {len(pin['eval'])} eval ids from {PIN}")
    try:
        train_rows, eval_rows = split_rows(kept, stratum, eval_eligible, pin=pin)
    except ValueError as e:
        print(f"\nX {e}")
        sys.exit(1)

    train_rows.sort(key=lambda r: (r["category"], r["id"]))
    eval_rows.sort(key=lambda r: (r["category"], r["id"]))

    def strip(r):
        return {"id": r["id"], "category": r["category"], "context": r["context"], "messages": r["messages"]}

    with open(f"{BASE}/train.jsonl", "w", encoding="utf-8") as f:
        for r in train_rows:
            f.write(json.dumps(strip(r), ensure_ascii=False) + "\n")
    with open(f"{BASE}/eval.jsonl", "w", encoding="utf-8") as f:
        for r in eval_rows:
            f.write(json.dumps(strip(r), ensure_ascii=False) + "\n")

    print(f"\nwrote {len(train_rows)} -> {BASE}/train.jsonl")
    print(f"wrote {len(eval_rows)} -> {BASE}/eval.jsonl")

    ecat = {}
    for r in eval_rows:
        ecat[r["category"]] = ecat.get(r["category"], 0) + 1
    print("\neval holdout by category:")
    for c in list(TARGET) + sorted(set(ecat) - set(TARGET)):
        print(f"  {c:10} {ecat.get(c,0)}")

    # deflection sub-type coverage in eval (em / mi / ot / fx from generated ids, plus exemplar <villager>-dNN)
    defl = [r for r in eval_rows if r["category"] == "deflection"]
    sub = {}
    for r in defl:
        m = re.match(rf"{VILLAGER}-def-(\w\w)-", r["id"])
        key = m.group(1) if m else ("exemplar" if re.match(rf"{VILLAGER}-d\d", r["id"]) else "other")
        sub[key] = sub.get(key, 0) + 1
    print(f"eval deflection sub-types: {sub}")


if __name__ == "__main__":
    main()

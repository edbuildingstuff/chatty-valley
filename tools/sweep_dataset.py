"""
Cross-batch sweep for a villager's authored dataset. Read-only.

build_batch.py gates ONE file: dashes, turn counts, within-file duplicate openers. The failures that
survive that are the ones that only exist between files, or that no regex in the build tool looks
for: an opener reused across two batches, a spouse endearment, a published novel, a numeric age, a
[end] marker outside farewell, a location string the runtime cannot produce, a category that drifted
off its target, or fifty rows that all open with the same word.

This runs those checks over every batch jsonl plus exemplars.jsonl, and prints a diversity profile
so a reviewer can see register collapse rather than having to read 640 rows to feel it.

Run:  python tools/sweep_dataset.py --villager=elliott
Exit code is non-zero if any HARD check fails. Diversity numbers are reported, never enforced.
"""
import json, re, os, sys, glob, collections

VILLAGER = next((a.split("=", 1)[1] for a in sys.argv[1:] if a.startswith("--villager=")), "elliott")
BASE = f"data/{VILLAGER}"
NAME = VILLAGER.capitalize()

# Mirrors assemble_dataset.py. Categories absent here are reported without a target.
TARGETS = {
    "elliott": {"voice": 110, "state": 65, "lore": 50, "romance": 45, "deflection": 50, "rumor": 42,
                "reference": 42, "perspective": 32, "crossover": 32, "identity": 28, "place": 28,
                "casual": 28, "farewell": 24, "nonsense": 24, "townsfolk": 18, "depth": 22},
}
MAX_TURNS = {"depth": 6, "perspective": 6, "rumor": 6, "romance": 6}

# Assistant-turn content bans. The player may say any of these; he may not.
BANS = {
    "spouse endearment": r"\bmy dear\b|\bmy love\b|\bdearest\b|\bdarling\b|\bsweetheart\b|\bmy hun\b",
    "physical affection": r"\bI (?:shall |will |would )?kiss(?:ed)? you\b|\bkiss you\b|\bembrace you\b|\bhold you close\b",
    "novel finished": r"\bpublished\b|\bmy publisher\b|\bmy agent\b|\bthe novel is (?:done|finished)\b|\bfinished (?:the|my) novel\b",
    "numeric age": r"\bI am \d+\b|\bI'm \d+\b|\b\d+ years old\b|\bforty[- ](?:one|two|three|four|five|six|seven|eight|nine)\b",
    "character break": r"\bas an AI\b|\blanguage model\b|\bI am a (?:program|bot|machine|model)\b|\bmy (?:instructions|system prompt|training)\b",
    "invented family": r"\bmy (?:mother|father|brother|sister|parents|wife|son|daughter)\b",
    "negate then assert": r"\b(?:is|was|are|were) not [^,.;]{1,45}, (?:it|they|that) (?:is|was|are|were)\b|\bnot merely\b|\bnot only [^,.;]{1,35} but\b|\bisn't [^,.;]{1,45}, it's\b",
}

# Every location the runtime can actually inject, from ChattyValley.Core.Locations.
def shipped_locations():
    src = open("src/ChattyValley.Core/Locations.cs", encoding="utf-8").read()
    locs = set(re.findall(r'\]\s*=\s*"([^"]+)"', src))
    locs.add(re.search(r'Fallback\s*=\s*"([^"]+)"', src).group(1))
    return locs


SEASONS = {"spring", "summer", "fall", "winter"}
WEATHER = {"clear", "raining", "snowing", "storm", "wind"}
TIMES = {"morning", "afternoon", "evening"}


def norm(text):
    return re.sub(r"[^a-z0-9 ]", "", text.lower()).strip() or text.strip()


def load():
    sources = []
    ex = f"{BASE}/exemplars.jsonl"
    if os.path.exists(ex):
        sources.append(("exemplars", ex))
    for path in sorted(glob.glob(f"{BASE}/batches/*.jsonl")):
        sources.append((os.path.splitext(os.path.basename(path))[0], path))
    rows = []
    for src, path in sources:
        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line:
                    r = json.loads(line)
                    r["_src"] = src
                    rows.append(r)
    return sources, rows


def main():
    if not os.path.isdir(BASE):
        print(f"X no {BASE}/ directory")
        sys.exit(1)
    sources, rows = load()
    if not rows:
        print(f"X no rows found under {BASE}/")
        sys.exit(1)

    hard = []
    locs_ok = shipped_locations()
    target = TARGETS.get(VILLAGER, {})

    print(f"villager: {VILLAGER}")
    print(f"sources: {len(sources)} ({', '.join(s for s, _ in sources)})")
    print(f"rows: {len(rows)}\n")

    # ---- ids, turn shape, context sanity -------------------------------------------------------
    seen_ids = {}
    for r in rows:
        rid = r["id"]
        if rid in seen_ids:
            hard.append(f"duplicate id {rid} ({r['_src']} and {seen_ids[rid]})")
        seen_ids[rid] = r["_src"]

        asst = [m for m in r["messages"] if m["role"] == "assistant"]
        usr = [m for m in r["messages"] if m["role"] == "user"]
        cap = MAX_TURNS.get(r.get("category") or "", 3)
        if not (2 <= len(asst) <= cap):
            hard.append(f"{rid}: {len(asst)} assistant turns (cap {cap} for {r['category']})")
        if len(usr) != len(asst):
            hard.append(f"{rid}: {len(usr)} user vs {len(asst)} assistant turns")
        seq = [m["role"] for m in r["messages"] if m["role"] != "system"]
        if seq != ["user", "assistant"] * len(asst):
            hard.append(f"{rid}: turn order is not clean user/assistant alternation")

        parts = [p.strip() for p in (r.get("context") or "").split(",")]
        if len(parts) < 4:
            hard.append(f"{rid}: malformed context {r.get('context')!r}")
            continue
        season, wt, loc, hearts = parts[0], parts[1], parts[2], parts[3]
        if season not in SEASONS:
            hard.append(f"{rid}: bad season {season!r}")
        wparts = wt.split()
        if len(wparts) != 2 or wparts[0] not in WEATHER or wparts[1] not in TIMES:
            hard.append(f"{rid}: bad weather/time {wt!r}")
        elif wparts[0] == "raining" and season == "winter":
            # Game truth rather than a setting-doc line: Stardew winter has snow or clear weather.
            hard.append(f"{rid}: raining in winter (the game has snow or clear)")
        elif wparts[0] in ("raining", "storm") and loc != "Elliott's cabin":
            # Setting doc section 4 is strict: "Rain, any season: inside his cabin ALL DAY." The only
            # canon exception is Green Rain in year 1, which the context line cannot express. So a
            # rainy library or a rainy Saloon is a canon defect, not merely an odd pairing.
            hard.append(f"{rid}: {wparts[0]} paired with {loc!r} (canon: rain means his cabin all day)")
        if loc not in locs_ok:
            hard.append(f"{rid}: location {loc!r} is not one the runtime can inject")
        if not re.fullmatch(r"\d+ hearts", hearts) or not (0 <= int(hearts.split()[0]) <= 14):
            hard.append(f"{rid}: bad hearts {hearts!r}")

        # system line must match PromptBuilder.BuildSystem in adapter mode
        want = f"You are {NAME}, a resident of Pelican Town in Stardew Valley. Current situation: "
        if not r["messages"][0]["content"].startswith(want):
            hard.append(f"{rid}: system line does not match PromptBuilder")

    # ---- content bans on assistant turns only --------------------------------------------------
    for label, pat in BANS.items():
        hits = [(r["id"], r["_src"], m["content"]) for r in rows
                for m in r["messages"] if m["role"] == "assistant" and re.search(pat, m["content"], re.I)]
        print(f"{label:20} {len(hits)}")
        for rid, src, c in hits[:6]:
            print(f"    {rid} [{src}]: {c[:100]}")
        if len(hits) > 6:
            print(f"    ... {len(hits) - 6} more")
        hard.extend(f"{label}: {rid} [{src}]" for rid, src, _ in hits)

    dashes = [(r["id"], r["_src"]) for r in rows for m in r["messages"] if "—" in m["content"] or "–" in m["content"]]
    print(f"{'em/en dash':20} {len(dashes)}")
    hard.extend(f"dash: {rid} [{src}]" for rid, src in dashes)

    # ---- [end] marker discipline ---------------------------------------------------------------
    bad_end = []
    for r in rows:
        asst = [m["content"] for m in r["messages"] if m["role"] == "assistant"]
        for i, c in enumerate(asst):
            if "[end]" in c and not (r["category"] == "farewell" and i == len(asst) - 1):
                bad_end.append(f"{r['id']} [{r['_src']}] turn {i+1}/{len(asst)} cat={r['category']}")
    print(f"{'[end] misplaced':20} {len(bad_end)}")
    for b in bad_end:
        print(f"    {b}")
    hard.extend(f"[end]: {b}" for b in bad_end)

    # ---- cross-file opener collisions ----------------------------------------------------------
    openers = collections.defaultdict(list)
    for r in rows:
        u = [m for m in r["messages"] if m["role"] == "user"]
        if u:
            openers[norm(u[0]["content"])].append((r["id"], r["_src"]))
    collisions = {k: v for k, v in openers.items() if len(v) > 1}
    cross = {k: v for k, v in collisions.items() if len({s for _, s in v}) > 1}
    print(f"\nopener collisions: {len(collisions)} total, {len(cross)} across different files")
    for k, v in list(cross.items())[:15]:
        print(f"    {k[:60]!r}: {', '.join(f'{i} [{s}]' for i, s in v)}")
    if len(cross) > 15:
        print(f"    ... {len(cross) - 15} more")
    # Cross-file collisions are dropped by assemble_dataset (first wins), so they cost rows
    # rather than corrupting the set. Reported loudly, not fatal.

    # ---- vantage notes (reported, never enforced) ----------------------------------------------
    # Some locations the runtime CAN inject are places Elliott's canon barely reaches. These are
    # judgement calls rather than defects, so they are surfaced with counts and left to a reviewer.
    # Setting doc section 4: the clinic is one checkup a year (Summer 9); the desert is the bus trip
    # to the Desert Festival; the community center appears in no schedule or vantage line at all.
    notes = collections.defaultdict(list)
    for r in rows:
        parts = [p.strip() for p in (r.get("context") or "").split(",")]
        if len(parts) < 4:
            continue
        season, loc, rest = parts[0], parts[2], " ".join(parts[4:])
        if loc == "the community center":
            notes["community center (no canon vantage)"].append(r["id"])
        elif loc == "the clinic" and season != "summer":
            notes["clinic outside summer (canon: Summer 9 checkup)"].append(r["id"])
        elif loc == "the desert" and "festival" not in rest.lower():
            notes["desert without a festival clause"].append(r["id"])
        elif loc == "the forest" and season != "summer" and "festival" not in rest.lower():
            notes["forest outside summer (canon: summer walk near Leah's)"].append(r["id"])
        elif loc == "the library" and season in ("spring", "summer") and "festival" not in rest.lower():
            notes["library in spring/summer (canon: fall and winter)"].append(r["id"])
    print("\nvantage notes (judgement calls, not failures):")
    if not notes:
        print("  none")
    for k in sorted(notes):
        ids = notes[k]
        print(f"  {len(ids):3}  {k}")
        print(f"       {', '.join(ids[:8])}{' ...' if len(ids) > 8 else ''}")

    # ---- category mix --------------------------------------------------------------------------
    cat = collections.Counter(r["category"] for r in rows)
    print("\ncategory mix (have vs target):")
    for c in sorted(set(cat) | set(target)):
        t = target.get(c)
        flag = ""
        if t:
            drift = (cat.get(c, 0) - t) / t
            flag = "  <-- off by more than 10%" if abs(drift) > 0.10 else ""
        print(f"  {c:12} {cat.get(c, 0):4} / {t if t else '(no target)'}{flag}")
    print(f"  {'TOTAL':12} {len(rows):4} / {sum(target.values()) if target else '?'}")

    # ---- diversity profile (reported, never enforced) ------------------------------------------
    print("\ndiversity profile:")
    first_words = collections.Counter()
    sent_counts = collections.Counter()
    for r in rows:
        for m in r["messages"]:
            if m["role"] == "assistant":
                w = re.sub(r"[^a-zA-Z]", "", m["content"].split()[0] if m["content"].split() else "")
                first_words[w.lower()] += 1
                t = m["content"].replace("...", "…")
                sent_counts[max(1, len([p for p in re.split(r"[.!?…]+", t) if p.strip()]))] += 1
    total_turns = sum(first_words.values())
    print(f"  assistant turns: {total_turns}")
    print(f"  sentences per reply: {dict(sorted(sent_counts.items()))}")
    print("  most common opening word of a reply:")
    for w, n in first_words.most_common(8):
        print(f"    {w or '(symbol)':12} {n:4}  {n/total_turns:5.1%}")
    player_openers = collections.Counter()
    for r in rows:
        u = [m for m in r["messages"] if m["role"] == "user"]
        if u:
            w = re.sub(r"[^a-zA-Z]", "", u[0]["content"].split()[0] if u[0]["content"].split() else "")
            player_openers[w.lower()] += 1
    print("  most common opening word of a player turn:")
    for w, n in player_openers.most_common(8):
        print(f"    {w or '(symbol)':12} {n:4}  {n/len(rows):5.1%}")

    print(f"\nHARD FAILURES: {len(hard)}")
    for h in hard[:40]:
        print(f"  X {h}")
    if len(hard) > 40:
        print(f"  ... {len(hard) - 40} more")
    sys.exit(1 if hard else 0)


if __name__ == "__main__":
    main()

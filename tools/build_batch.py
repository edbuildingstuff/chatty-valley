"""
Parse a hand-authored batch markdown file (same format as linus-exemplars.md) into JSONL training rows,
running the dataset-plan quality gates: dash-lint (no em/en dash), reply length (1 to 3 sentences),
turn count (2 to 3 assistant turns; the depth category allows 2 to 6), context present, and duplicate
detection.

The villager is derived from the batch path (data/<villager>/batches/<name>.md), so the tool is
villager-generic: it sets both the system line and the expected speaker tag from that name. Pass
--villager to override when the path does not carry it.

Usage:
    python tools/build_batch.py data/linus/batches/deflection.md
    python tools/build_batch.py data/elliott/batches/romance.md
    # -> writes <batch>.jsonl next to the markdown and prints a report.
Exit code is non-zero if any hard gate fails (dash, turn count out of range, missing context, duplicate).
"""
import json, re, os, sys, hashlib

# The system line MUST match ChattyValley.Core PromptBuilder.BuildSystem in adapter mode, which
# renders "You are {c.Name}, a resident of Pelican Town in Stardew Valley. Current situation: {ctx}".
# Training format equals inference format, so this template is shared, not parallel.
SYSTEM = "You are {name}, a resident of Pelican Town in Stardew Valley. Current situation: {ctx}"
DASHES = ("—", "–")  # em dash, en dash
# The depth batch exists to make deep multi-turn conversation in-distribution (the v1 model, trained
# only on 2 to 3 turns, degenerated several turns into an in-game chat), so its rows run longer.
# The perspective batch (v4) trains holding an epistemic boundary under SUSTAINED third-party
# probing, so its rows also run longer than the 2-to-3-turn default.
MAX_TURNS = {"depth": 6, "perspective": 6, "rumor": 6}  # category -> max assistant turns (default 3)


def villager_from_path(md_path):
    """data/elliott/batches/romance.md -> "elliott". Falls back to the parent-of-parent dir name."""
    parts = os.path.normpath(md_path).split(os.sep)
    if "batches" in parts:
        i = parts.index("batches")
        if i > 0:
            return parts[i - 1]
    return parts[-3] if len(parts) >= 3 else "linus"


def parse(md_path, villager):
    speaker_re = re.compile(rf"^-\s*{re.escape(villager)}:\s*(.*)", re.IGNORECASE)
    system = SYSTEM.format(name=villager.capitalize(), ctx="{ctx}")
    rows, cur, cat = [], None, None
    with open(md_path, encoding="utf-8") as f:
        for line in f:
            s = line.rstrip("\n")
            m = re.match(r"^##\s+.*\[cat:(\w+)\]", s)
            if m:
                cat = m.group(1); continue
            m = re.match(r"^###\s+(\S+)", s)
            if m:
                if cur: rows.append(cur)
                cur = {"id": m.group(1), "category": cat, "context": None, "messages": []}
                continue
            if cur is None:
                continue
            m = re.match(r"^context:\s*(.*)", s)
            if m:
                cur["context"] = m.group(1).strip()
                cur["messages"].append({"role": "system", "content": system.format(ctx=cur["context"])})
                continue
            m = re.match(r"^-\s*player:\s*(.*)", s)
            if m:
                cur["messages"].append({"role": "user", "content": m.group(1).strip()}); continue
            m = speaker_re.match(s)
            if m:
                cur["messages"].append({"role": "assistant", "content": m.group(1).strip()}); continue
    if cur:
        rows.append(cur)
    return rows


def count_sentences(text):
    # split on sentence-ending punctuation, treating runs (e.g. "...", "?!") as one terminator;
    # ellipsis "..." mid-line is collapsed first so it does not over-count.
    t = text.replace("...", "…")
    parts = [p for p in re.split(r"[.!?…]+", t) if p.strip()]
    return max(1, len(parts))


def validate(rows):
    hard, soft, seen = [], [], {}
    for r in rows:
        rid = r["id"]
        asst = [m for m in r["messages"] if m["role"] == "assistant"]
        usr = [m for m in r["messages"] if m["role"] == "user"]
        if not r["context"]:
            hard.append(f"{rid}: missing context")
        max_turns = MAX_TURNS.get(r["category"] or "", 3)
        if not (2 <= len(asst) <= max_turns):
            hard.append(f"{rid}: {len(asst)} assistant turns (want 2 to {max_turns})")
        if len(usr) != len(asst):
            hard.append(f"{rid}: {len(usr)} user vs {len(asst)} assistant turns (should alternate evenly)")
        for m in r["messages"]:
            for d in DASHES:
                if d in m["content"]:
                    hard.append(f"{rid}: contains dash {d!r} in {m['role']} turn")
        # Length is reported as a distribution (see sent_hist), not enforced: Linus's short full-stop
        # cadence is part of the voice, so 4 short sentences is fine. Only flag a genuinely runaway
        # reply worth a second look.
        for i, m in enumerate(asst):
            n = count_sentences(m["content"])
            if n > 5:
                soft.append(f"{rid} reply {i+1}: {n} sentences (unusually long, worth a look)")
        # duplicate detection on the first user turn (the jailbreak/prompt), normalized. Symbol-only
        # openers (the nonsense batch: ";;;;", "....", "%%%") normalize to empty; fall back to the raw
        # text so distinct symbol noise is not treated as one opener.
        if usr:
            key = re.sub(r"[^a-z0-9 ]", "", usr[0]["content"].lower()).strip() or usr[0]["content"].strip()
            if key in seen:
                hard.append(f"{rid}: duplicate opening user turn shared with {seen[key]}")
            else:
                seen[key] = rid
    return hard, soft


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    flags = {a.split("=")[0]: a.split("=", 1)[-1] for a in sys.argv[1:] if a.startswith("--")}
    md_path = args[0] if args else "data/linus/batches/deflection.md"
    villager = flags.get("--villager") or villager_from_path(md_path)
    out_path = os.path.splitext(md_path)[0] + ".jsonl"
    rows = parse(md_path, villager)

    # A wrong speaker tag matches nothing, and every row then fails the turn-count gate as if the
    # AUTHORING were broken. Name the real cause instead: this is how a Linus-hardcoded tool would
    # have silently produced turnless rows for villager #2.
    if rows and not any(m["role"] == "assistant" for r in rows for m in r["messages"]):
        print(f"batch: {md_path}")
        print(f"X no '{villager}:' speaker lines matched in {len(rows)} parsed rows.")
        print(f"  Expected reply lines shaped '- {villager}: ...'. Pass --villager=<name> if the "
              f"path does not carry the villager.")
        sys.exit(1)

    hard, soft = validate(rows)

    turn_hist, sent_hist = {}, {}
    for r in rows:
        t = sum(1 for m in r["messages"] if m["role"] == "assistant")
        turn_hist[t] = turn_hist.get(t, 0) + 1
        for m in r["messages"]:
            if m["role"] == "assistant":
                n = count_sentences(m["content"])
                sent_hist[n] = sent_hist.get(n, 0) + 1

    print(f"batch: {md_path}")
    print(f"villager: {villager}")
    print(f"rows parsed: {len(rows)}")
    print(f"categories: {sorted({r['category'] for r in rows})}")
    print(f"assistant turns per row: {dict(sorted(turn_hist.items()))}")
    print(f"sentences per assistant reply: {dict(sorted(sent_hist.items()))}")
    subtypes = {}
    for r in rows:
        st = "-".join(r["id"].split("-")[:3])  # <villager>-def-em, etc.
        subtypes[st] = subtypes.get(st, 0) + 1
    print(f"sub-type counts: {subtypes}")
    print(f"soft flags (length): {len(soft)}")
    for s in soft:
        print(f"  ~ {s}")
    print(f"hard failures: {len(hard)}")
    for h in hard:
        print(f"  X {h}")

    if hard:
        print("\nNOT writing jsonl until hard failures are fixed.")
        sys.exit(1)

    with open(out_path, "w", encoding="utf-8") as f:
        for r in rows:
            f.write(json.dumps(r, ensure_ascii=False) + "\n")
    print(f"\nwrote {len(rows)} rows -> {out_path}")


if __name__ == "__main__":
    main()

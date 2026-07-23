"""
Parse a hand-authored batch markdown file (same format as linus-exemplars.md) into JSONL training rows,
running the dataset-plan quality gates: dash-lint (no em/en dash), reply length (1 to 3 sentences),
turn count (2 to 3 assistant turns; the depth category allows 2 to 6), context present, and duplicate
detection.

Usage:
    python tools/build_batch.py data/linus/batches/deflection.md
    # -> writes data/linus/batches/deflection.jsonl and prints a report.
Exit code is non-zero if any hard gate fails (dash, turn count out of range, missing context, duplicate).
"""
import json, re, os, sys, hashlib

SYSTEM = "You are Linus, a resident of Pelican Town in Stardew Valley. Current situation: {ctx}"
DASHES = ("—", "–")  # em dash, en dash
# The depth batch exists to make deep multi-turn conversation in-distribution (the v1 model, trained
# only on 2 to 3 turns, degenerated several turns into an in-game chat), so its rows run longer.
# The perspective batch (v4) trains holding an epistemic boundary under SUSTAINED third-party
# probing, so its rows also run longer than the 2-to-3-turn default.
MAX_TURNS = {"depth": 6, "perspective": 6, "rumor": 6}  # category -> max assistant turns (default 3)


def parse(md_path):
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
                cur["messages"].append({"role": "system", "content": SYSTEM.format(ctx=cur["context"])})
                continue
            m = re.match(r"^-\s*player:\s*(.*)", s)
            if m:
                cur["messages"].append({"role": "user", "content": m.group(1).strip()}); continue
            m = re.match(r"^-\s*linus:\s*(.*)", s)
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
    md_path = sys.argv[1] if len(sys.argv) > 1 else "data/linus/batches/deflection.md"
    out_path = os.path.splitext(md_path)[0] + ".jsonl"
    rows = parse(md_path)
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
    print(f"rows parsed: {len(rows)}")
    print(f"categories: {sorted({r['category'] for r in rows})}")
    print(f"assistant turns per row: {dict(sorted(turn_hist.items()))}")
    print(f"sentences per assistant reply: {dict(sorted(sent_hist.items()))}")
    subtypes = {}
    for r in rows:
        st = "-".join(r["id"].split("-")[:3])  # linus-def-em, etc.
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

"""
Convert a hand-authored <villager>-exemplars.md into exemplars.jsonl (the training row format).
Each row: {id, category, context, messages:[{system}, {user}, {assistant}, ...]}. The system content is
minimal (identity + the game-state context line), because the voice lives in the fine-tuned weights.

Villager-generic: pass --villager=<name> (default linus). The system line and the expected speaker tag
both come from that name, and the system line matches PromptBuilder.BuildSystem in adapter mode so
training format equals inference format.

Run:  python tools/exemplars_to_jsonl.py
      python tools/exemplars_to_jsonl.py --villager=elliott
"""
import json, re, os, sys

VILLAGER = next((a.split("=", 1)[1] for a in sys.argv[1:] if a.startswith("--villager=")), "linus")
base = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "data", VILLAGER))
md = os.path.join(base, f"{VILLAGER}-exemplars.md")
out = os.path.join(base, "exemplars.jsonl")

SYSTEM = f"You are {VILLAGER.capitalize()}, a resident of Pelican Town in Stardew Valley. Current situation: {{ctx}}"
SPEAKER_RE = re.compile(rf"^-\s*{re.escape(VILLAGER)}:\s*(.*)", re.IGNORECASE)

rows, cur, cat = [], None, None
with open(md, encoding="utf-8") as f:
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
        m = SPEAKER_RE.match(s)
        if m:
            cur["messages"].append({"role": "assistant", "content": m.group(1).strip()}); continue
if cur:
    rows.append(cur)

if rows and not any(m["role"] == "assistant" for r in rows for m in r["messages"]):
    print(f"X no '{VILLAGER}:' speaker lines matched. Expected reply lines shaped '- {VILLAGER}: ...'.")
    sys.exit(1)

with open(out, "w", encoding="utf-8") as f:
    for r in rows:
        f.write(json.dumps(r, ensure_ascii=False) + "\n")

by_cat, turn_hist = {}, {}
for r in rows:
    by_cat[r["category"]] = by_cat.get(r["category"], 0) + 1
    t = sum(1 for m in r["messages"] if m["role"] == "assistant")
    turn_hist[t] = turn_hist.get(t, 0) + 1

print("villager:", VILLAGER)
print("source:", md)
print("exemplars:", len(rows))
print("by category:", by_cat)
print("assistant turns per row:", dict(sorted(turn_hist.items())))
bad = [r["id"] for r in rows if not r["context"] or sum(1 for m in r["messages"] if m["role"] == "assistant") < 2]
print("rows needing attention (missing context or <2 turns):", bad or "none")

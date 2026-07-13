"""
Step 1 of the Linus dataset: clean the raw canon dialogue into training-ready utterances.

Reads data/linus/raw/*.json (verbatim game text with dialogue markup) and produces
data/linus/canon-clean.jsonl: one record per spoken segment, with the markup parsed out and
emotion / condition captured as metadata. Multi-box lines are SPLIT on #$e# / #$b# into separate
segments (Edward's call: nicer to handle, matches the tight on-screen dialogue). Branching
question lines ($y / $q) are kept whole and flagged, since they are natural multi-turn seeds.
"""
import json, re, os

base = r"C:/Users/edwar/Documents/ertas_dev/chatty-valley/data/linus"
raw = os.path.join(base, "raw")

def load(n):
    with open(os.path.join(raw, n), encoding="utf-8") as f:
        return json.load(f)

EMO = {"h": "happy", "s": "sad", "u": "unhappy", "l": "love", "a": "angry"}

def clean_seg(seg):
    cond = None
    m = re.match(r"^\s*#\$(\S+)\s+([^#]*)#", seg)   # leading conditional, e.g. #$1 linusVandal#
    if m:
        cond = m.group(2).strip()
        seg = seg[m.end():]
    emos = re.findall(r"\$([hsula])(?![A-Za-z])", seg)  # emotion tokens ($h $s $u $l $a)
    seg = re.sub(r"#\$[^#]*#", " ", seg)   # any leftover control blocks
    seg = seg.replace("#", " ")
    seg = re.sub(r"\$[A-Za-z0-9]+", "", seg)  # remaining emotion / portrait / control tokens
    seg = re.sub(r"\[\d+\]", "", seg)          # item icon tokens like [166]
    if "^" in seg:
        seg = seg.split("^")[0]                # gender split -> male form
    seg = re.sub(r"\s+", " ", seg).strip()
    return seg, cond, (EMO.get(emos[-1]) if emos else None)

records = []

def add(source, key, line):
    if not line:
        return
    if "$y" in line or "$q" in line:           # branching question: keep whole, flag for multi-turn authoring
        records.append({"source": source, "key": key, "index": 0, "type": "branch",
                        "text": line.strip(), "emotion": None, "condition": None})
        return
    for i, part in enumerate(re.split(r"#\$[eb]#", line)):
        text, cond, emo = clean_seg(part)
        if text:
            records.append({"source": source, "key": key, "index": i, "type": "line",
                            "text": text, "emotion": emo, "condition": cond})

for k, v in load("Linus.json").items():
    add("main", k, v)
for k, v in load("rainy_Linus.json").items():
    add("rainy", k, v)
for k, v in load("festivals_Linus.json").items():
    add("festival", k, v)
for ev, lines in load("events_Linus.json").items():
    for j, l in enumerate(lines):
        add("event", f"{ev}#{j}", l)

with open(os.path.join(base, "canon-clean.jsonl"), "w", encoding="utf-8") as f:
    for r in records:
        f.write(json.dumps(r, ensure_ascii=False) + "\n")

lines = [r for r in records if r["type"] == "line"]
branches = [r for r in records if r["type"] == "branch"]
by_source = {}
for r in lines:
    by_source[r["source"]] = by_source.get(r["source"], 0) + 1

print("clean segments:", len(lines), "| branches:", len(branches))
print("by source     :", by_source)
print("with condition:", sum(1 for r in lines if r["condition"]),
      "| with emotion:", sum(1 for r in lines if r["emotion"]))
print("avg words/seg :", round(sum(len(r["text"].split()) for r in lines) / len(lines), 1))
print("\nsample cleaned segments:")
for r in lines[:8]:
    tag = f"  [{r['emotion']}]" if r["emotion"] else ""
    cond = f"  (cond: {r['condition']})" if r["condition"] else ""
    print(f"  - {r['text']}{tag}{cond}")
print("\nbranch entries (multi-turn seeds):", [r["key"] for r in branches])

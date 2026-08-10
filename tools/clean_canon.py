"""
Clean a villager's raw canon dialogue into training-ready utterances.

Usage: python tools/clean_canon.py <Villager>   (e.g. Linus, Elliott)

Reads data/<villager>/raw/*.json (verbatim game text with dialogue markup) and produces
data/<villager>/canon-clean.jsonl: one record per spoken segment, with the markup parsed out and
emotion / condition captured as metadata. Multi-box lines are SPLIT on #$e# / #$b# into separate
segments, which are nicer to handle and match the tight on-screen dialogue. Branching
question lines ($y / $q) are kept whole and flagged, since they are natural multi-turn seeds.

Recognised raw files (any subset may exist; marriage candidates have all six):
    <Villager>.json                  -> source "main"
    rainy_<Villager>.json            -> source "rainy"
    festivals_<Villager>.json        -> source "festival"
    events_<Villager>.json           -> source "event"      (values are lists of lines)
    MarriageDialogue<Villager>.json  -> source "marriage"
    engagement_<Villager>.json       -> source "engagement"
"""
import json, re, os, sys

if len(sys.argv) != 2:
    sys.exit("usage: python tools/clean_canon.py <Villager>")
villager = sys.argv[1]

base = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "data", villager.lower()))
raw = os.path.join(base, "raw")

SOURCES = [
    (f"{villager}.json", "main"),
    (f"rainy_{villager}.json", "rainy"),
    (f"festivals_{villager}.json", "festival"),
    (f"events_{villager}.json", "event"),
    (f"MarriageDialogue{villager}.json", "marriage"),
    (f"engagement_{villager}.json", "engagement"),
]

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

for fname, source in SOURCES:
    if not os.path.exists(os.path.join(raw, fname)):
        continue
    data = load(fname)
    for k, v in data.items():
        if isinstance(v, list):                # event files: key -> list of lines
            for j, l in enumerate(v):
                add(source, f"{k}#{j}", l)
        else:
            add(source, k, v)

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

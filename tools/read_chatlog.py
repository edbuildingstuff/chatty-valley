"""
Pretty-print Chatty Valley in-game chat transcripts (the mod's development JSONL logs) for
post-playthrough debugging.

The mod (ChatLogEnabled, on by default) appends one JSON object per event to
  <mod folder>/chat-logs/chatlog-YYYY-MM-DD.jsonl
Default mod folder: C:/Program Files (x86)/Steam/steamapps/common/Stardew Valley/Mods/ChattyValley.Mod

Usage:
    python tools/read_chatlog.py                      # today's log from the default mod folder
    python tools/read_chatlog.py <path/to/chatlog.jsonl>
    python tools/read_chatlog.py --last 3             # only the last N conversations
    python tools/read_chatlog.py --flags              # only conversations with flagged turns

Flags printed per turn:
    [GUARD]   the sidecar's word-run collapse changed the model's raw output (raw is shown)
    [RUN n]   the shown reply still contains a same-word run of n >= 2
    [SLOW]    reply took longer than 3 seconds
"""
import argparse, glob, json, os, re, sys
from datetime import date

DEFAULT_DIR = (r"C:/Program Files (x86)/Steam/steamapps/common/Stardew Valley"
               r"/Mods/ChattyValley.Mod/chat-logs")


def longest_word_run(text):
    words = [w.strip(".,!?;:\"'").lower() for w in text.split()]
    best = cur = 0
    prev = None
    for w in (w for w in words if w):
        cur = cur + 1 if w == prev else 1
        prev = w
        best = max(best, cur)
    return best


def find_log(arg):
    if arg:
        return arg
    today = os.path.join(DEFAULT_DIR, f"chatlog-{date.today():%Y-%m-%d}.jsonl")
    if os.path.exists(today):
        return today
    logs = sorted(glob.glob(os.path.join(DEFAULT_DIR, "chatlog-*.jsonl")))
    if logs:
        return logs[-1]
    sys.exit(f"no chat logs found in {DEFAULT_DIR}")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("log", nargs="?", help="path to a chatlog .jsonl (default: newest in the mod folder)")
    ap.add_argument("--last", type=int, default=0, help="only show the last N conversations")
    ap.add_argument("--flags", action="store_true", help="only show conversations with flagged turns")
    args = ap.parse_args()

    path = find_log(args.log)
    convos, order = {}, []
    for line in open(path, encoding="utf-8"):
        line = line.strip()
        if not line:
            continue
        r = json.loads(line)
        cid = r.get("convo", "?")
        if cid not in convos:
            convos[cid] = {"start": None, "turns": [], "end": None}
            order.append(cid)
        if r["evt"] == "start":
            convos[cid]["start"] = r
        elif r["evt"] == "turn":
            convos[cid]["turns"].append(r)
        elif r["evt"] == "end":
            convos[cid]["end"] = r

    if args.last:
        order = order[-args.last:]

    print(f"log: {path}  ({len(order)} conversation(s) shown)\n")
    for cid in order:
        c = convos[cid]
        s = c["start"] or {}
        flagged_lines, lines = 0, []
        lines.append("=" * 78)
        lines.append(f"[{cid}] {s.get('ts','?')}  {s.get('npc','?')}  |  {s.get('gameTime','')}")
        lines.append(f"  model: {s.get('baseModel','?')} + {s.get('adapter','?')}  "
                     f"temp={s.get('temp')} rp={s.get('repeatPenalty')} fp={s.get('frequencyPenalty')} "
                     f"window={s.get('window')}")
        if s.get("system"):
            lines.append(f"  system: {s['system']}")
        for t in c["turns"]:
            flags = []
            if t.get("raw"):
                flags.append("GUARD")
            run = longest_word_run(t.get("reply") or "")
            if run >= 2:
                flags.append(f"RUN {run}")
            if t.get("ms", 0) > 3000:
                flags.append("SLOW")
            flag = ("  <-- " + " ".join(flags)) if flags else ""
            if flags:
                flagged_lines += 1
            lines.append(f"  You  : {t.get('player','')}")
            lines.append(f"  NPC  : {t.get('reply','')}   [{t.get('ms','?')} ms, "
                         f"{t.get('sentToModel','?')}/{t.get('historyLen','?')} msgs sent]{flag}")
            if t.get("raw"):
                lines.append(f"  raw  : {t['raw']}")
            if t.get("prompt"):
                lines.append(f"  prompt: {t['prompt'][:400]}...")
        if not c["end"]:
            lines.append("  (conversation had no end event: game closed mid-chat?)")
        if args.flags and not flagged_lines:
            continue
        print("\n".join(lines))
        print()


if __name__ == "__main__":
    main()

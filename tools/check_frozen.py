"""
Verify a rewrite changed assistant text only.

Compares every data/<villager>/batches/*.jsonl and data/<villager>/exemplars.jsonl at a git ref
against the working tree: ids, categories, contexts, turn roles and every non-assistant message
must match. New ids are allowed (new blocks append to existing batch files); a missing id is not.

Usage:
    python tools/check_frozen.py <git-ref> [--villager=elliott]
"""
import glob, json, os, subprocess, sys


def frozen_view(r):
    return {"category": r.get("category"), "context": r.get("context"),
            "roles": [m["role"] for m in r["messages"]],
            "non_assistant": [m["content"] for m in r["messages"] if m["role"] != "assistant"]}


def compare(before_rows, after_rows):
    after = {r["id"]: r for r in after_rows}
    problems, changed = [], 0
    for rb in before_rows:
        ra = after.get(rb["id"])
        if ra is None:
            problems.append(f"{rb['id']}: missing after the rewrite")
            continue
        fb, fa = frozen_view(rb), frozen_view(ra)
        for field in fb:
            if fb[field] != fa[field]:
                problems.append(f"{rb['id']}: frozen field '{field}' changed")
        for mb, ma in zip(rb["messages"], ra["messages"]):
            if mb["role"] == "assistant" and mb["content"] != ma["content"]:
                changed += 1
    return problems, changed


def rows_from(text):
    return [json.loads(l) for l in text.splitlines() if l.strip()]


def at_ref(ref, path):
    p = subprocess.run(["git", "show", f"{ref}:{path}"], capture_output=True, encoding="utf-8")
    return rows_from(p.stdout) if p.returncode == 0 else None


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    if not args:
        print(__doc__)
        sys.exit(2)
    ref = args[0]
    villager = next((a.split("=", 1)[1] for a in sys.argv[1:] if a.startswith("--villager=")), "elliott")
    base = f"data/{villager}"
    paths = sorted(glob.glob(f"{base}/batches/*.jsonl")) + [f"{base}/exemplars.jsonl"]
    all_problems, total = [], 0
    for path in paths:
        path = path.replace(os.sep, "/")
        before = at_ref(ref, path)
        if before is None:
            print(f"  {path}: new file (not at {ref}), skipped")
            continue
        with open(path, encoding="utf-8") as f:
            after = rows_from(f.read())
        problems, changed = compare(before, after)
        total += changed
        print(f"  {path}: {len(before)} rows at {ref}, {changed} assistant turns changed, {len(problems)} problems")
        all_problems += [f"{path} {p}" for p in problems]
    print(f"\nassistant turns changed: {total}")
    print(f"PROBLEMS: {len(all_problems)}")
    for p in all_problems[:40]:
        print(f"  X {p}")
    sys.exit(1 if all_problems else 0)


if __name__ == "__main__":
    main()

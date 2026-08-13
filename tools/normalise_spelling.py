"""Normalise a villager's authored corpus to American spelling. Read-write, idempotent.

Stardew Valley is an American game and its English text carries no British spelling at all: across
the unpacked Content/Strings and Characters/Dialogue files it is color 38, theater 29, favorite 19,
flavor 18, honor 16, neighbor 9, defense 7, with zero of the British variants. The season word is
"fall" 38 times against a single "autumn", and that one is the cooked dish Autumn's Bounty.

Elliott settles it in his own voice. His canon file says "I'm honored that you would remember my
birthday", "another gray hair", "it would be an honor", and spells his signature exclamation
"Marvelous" with one L. His register reads faintly Victorian, which is most likely why parallel
authors drifted British; the spelling underneath it is American.

Operates on the authored markdown, which is the source of truth. Rebuild and reassemble afterwards;
the tool prints the exact commands for the files it touched.

    python tools/normalise_spelling.py --villager=elliott
    python tools/normalise_spelling.py --villager=elliott --check     # report only, exit 1 if dirty
    python tools/normalise_spelling.py --self-test                    # the conversion table

SUFFIXES ARE ENUMERATED, NOT OPEN. An open `[a-z]*` tail looks tidy and quietly turns "organist"
into "organizt", "realism" into "realizm" and a greyhound into a grayhound. Every rule below names
the endings it accepts, and the self-test pins both the conversions and the words to leave alone.
"""
import argparse
import glob
import os
import re
import sys

# Families where any lowercase tail is safe: no English word starts with these stems and continues
# into something that must not be converted.
OPEN_STEMS = {
    "favourite": "favorite",
    "neighbour": "neighbor",
    "marvellous": "marvelous",
    "jewellery": "jewelry",
    "aluminium": "aluminum",
    "favour": "favor",
    "colour": "color",
    "honour": "honor",
    "flavour": "flavor",
    "humour": "humor",
    "rumour": "rumor",
    "harbour": "harbor",
    "labour": "labor",
    "defence": "defense",
    "theatre": "theater",
    "sombre": "somber",
    "storey": "story",
    "plough": "plow",
    "metre": "meter",
}

# Families needing an explicit ending, because the stem is a live prefix of American words.
# ("realis" is in "realism"; "organis" is in "organist"; "grey" is in "greyhound".)
SUFFIXED = [
    (r"(recognis|apologis|organis|realis|civilis|criticis|memoris|specialis)"
     r"(e|es|ed|ing|able|ation|ations)", lambda stem: stem[:-1] + "z"),
    (r"(travell)(ed|ing|er|ers)", lambda stem: "travel"),
    (r"(practis)(e|es|ed|ing)", lambda stem: "practic"),
    (r"(cos)(y|ier|iest|ily|iness)", lambda stem: "coz"),
    (r"(grey)(|s|er|est|ish|ed|ing)", lambda stem: "gray"),
]

OPEN_PATTERN = re.compile(
    r"\b(" + "|".join(sorted(OPEN_STEMS, key=len, reverse=True)) + r")([a-z]*)\b", re.IGNORECASE)
SUFFIXED_PATTERNS = [(re.compile(r"\b" + pat + r"\b", re.IGNORECASE), fn) for pat, fn in SUFFIXED]


def restore_case(source: str, replacement: str) -> str:
    """Match the original's capitalisation. The corpus is lower and Title only, but a silent case
    flip inside a reply would be a subtle voice defect, so ALL CAPS is covered too."""
    if source.isupper():
        return replacement.upper()
    if source[0].isupper():
        return replacement[0].upper() + replacement[1:]
    return replacement


def convert(text: str):
    """Return (converted, [(before, after), ...])."""
    changes = []

    def open_swap(m):
        stem, suffix = m.group(1), m.group(2)
        after = restore_case(stem, OPEN_STEMS[stem.lower()]) + suffix
        changes.append((m.group(0), after))
        return after

    text = OPEN_PATTERN.sub(open_swap, text)

    for pattern, american_of in SUFFIXED_PATTERNS:
        def suffixed_swap(m, american_of=american_of):
            stem, suffix = m.group(1), m.group(2)
            after = restore_case(stem, american_of(stem.lower())) + suffix
            changes.append((m.group(0), after))
            return after
        text = pattern.sub(suffixed_swap, text)

    return text, changes


SELF_TEST = {
    # British in, American out
    "favourite": "favorite", "Favourite": "Favorite", "FAVOURITE": "FAVORITE",
    "favours": "favors", "favoured": "favored", "favourable": "favorable",
    "neighbour": "neighbor", "neighbours": "neighbors", "neighbouring": "neighboring",
    "colour": "color", "colourful": "colorful", "coloured": "colored",
    "honour": "honor", "honoured": "honored", "honourable": "honorable",
    "humour": "humor", "humourless": "humorless", "flavours": "flavors",
    "rumours": "rumors", "labourer": "laborer", "harbour": "harbor",
    "defence": "defense", "defences": "defenses", "theatre": "theater",
    "recognise": "recognize", "recognised": "recognized", "recognising": "recognizing",
    "apologise": "apologize", "apologising": "apologizing", "apologised": "apologized",
    "organised": "organized", "realised": "realized", "civilised": "civilized",
    "grey": "gray", "greyer": "grayer", "greyish": "grayish",
    "cosy": "cozy", "cosier": "cozier", "cosiest": "coziest", "cosily": "cozily",
    "practise": "practice", "practising": "practicing",
    "travelled": "traveled", "travelling": "traveling",
    "marvellous": "marvelous", "jewellery": "jewelry", "sombre": "somber", "metres": "meters",
    "Grey skies and a cosy fire": "Gray skies and a cozy fire",
    # must survive untouched: American already, or a different word that merely starts the same way
    "favorite": "favorite", "color": "color", "honor": "honor", "gray": "gray",
    "realism": "realism", "realist": "realist", "realistic": "realistic",
    "organist": "organist", "organism": "organism", "greyhound": "greyhound",
    "practice": "practice", "theatrical": "theatrical", "metric": "metric",
    "cost": "cost", "cosmic": "cosmic", "greet": "greet", "traveler": "traveler",
}


def self_test():
    bad = []
    for source, want in SELF_TEST.items():
        got, _ = convert(source)
        if got != want:
            bad.append(f"  {source!r}: expected {want!r}, got {got!r}")
        # idempotence: a second pass must be a no-op
        again, changes = convert(got)
        if changes or again != got:
            bad.append(f"  {source!r}: not idempotent, {got!r} -> {again!r}")
    if bad:
        print(f"SELF-TEST FAILED ({len(bad)}):")
        print("\n".join(bad))
        sys.exit(1)
    print(f"self-test: {len(SELF_TEST)} cases pass, conversion is idempotent")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--villager")
    ap.add_argument("--check", action="store_true", help="report without writing; exit 1 if dirty")
    ap.add_argument("--self-test", action="store_true", help="run the conversion table and exit")
    args = ap.parse_args()

    if args.self_test:
        self_test()
        return
    if not args.villager:
        raise SystemExit("--villager is required (or pass --self-test)")

    self_test()
    base = f"data/{args.villager}"
    if not os.path.isdir(base):
        raise SystemExit(f"no {base}/ directory")

    targets = sorted(glob.glob(f"{base}/batches/*.md"))
    targets += sorted(glob.glob(f"{base}/{args.villager}-exemplars.md"))
    if not targets:
        raise SystemExit(f"no authored markdown under {base}/")

    total, touched = 0, []
    for path in targets:
        with open(path, encoding="utf-8", newline="") as fh:
            original = fh.read()
        converted, changes = convert(original)
        if not changes:
            continue
        total += len(changes)
        touched.append(os.path.basename(path))
        seen = {}
        for before, after in changes:
            key = f"{before} -> {after}"
            seen[key] = seen.get(key, 0) + 1
        detail = ", ".join(f"{k} x{v}" if v > 1 else k for k, v in seen.items())
        print(f"  {os.path.basename(path):22} {len(changes):>2}  {detail}")
        if not args.check:
            with open(path, "w", encoding="utf-8", newline="") as fh:
                fh.write(converted)

    if not total:
        print(f"{args.villager}: already American throughout")
        return
    verb = "would change" if args.check else "changed"
    print(f"\n{args.villager}: {verb} {total} spelling(s) across {len(touched)} file(s)")
    if args.check:
        sys.exit(1)
    print("\nnow rebuild and reassemble:")
    for name in touched:
        if name.endswith("-exemplars.md"):
            print(f"  python tools/exemplars_to_jsonl.py --villager={args.villager}")
        else:
            print(f"  python tools/build_batch.py {base}/batches/{name}")
    print(f"  python tools/assemble_dataset.py --villager={args.villager}")
    print(f"  python tools/sweep_dataset.py --villager={args.villager}")


if __name__ == "__main__":
    main()

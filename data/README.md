# Data: extracting canonical villager dialogue

The Ertas dataset for each villager is grounded in that villager's **exact canon dialogue** from the
installed game. This folder holds the extracted source text and (later) the authored, enriched training rows.

## How the game stores dialogue

Stardew Valley 1.6 keeps data, maps, and text in compressed `.xnb` files under its `Content` folder
(e.g. `Content/Characters/Dialogue/Linus.xnb`). Each dialogue file is a `Dictionary<string,string>`
(dialogue key to line). In 1.6 these are **LZ4-compressed** (XNB flag `0x80`), not plain. Reference:
the Stardew wiki, [Modding:Editing XNB files](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files).

## How we extract it (`tools/DialogueDump`)

The wiki's recommended unpacker, **StardewXnbHack**, requires installing SMAPI, must run from inside the
game folder, and unpacks the *entire* Content folder. We do not need any of that. Instead, `tools/DialogueDump`
loads a single asset through the **game's own MonoGame `ContentManager`**, referenced straight from the
install (`MonoGame.Framework.dll`). That means:

- the exact same decompression and deserialization the game itself uses (correct by construction),
- no SMAPI, no XNB or LZ4 reverse-engineering,
- nothing written into the Steam install, and only the files we ask for.

### Reproduce

```powershell
dotnet build tools/DialogueDump/DialogueDump.csproj -c Release
$exe = "tools/DialogueDump/bin/Release/net10.0/DialogueDump.exe"

# one asset (name is relative to Content, no .xnb):
& $exe "Characters\Dialogue\Linus" "data/linus/raw/Linus.json"

# every English file in a folder:
& $exe --dir "Data\Festivals" "<outDir>"
& $exe --dir "Data\Events"    "<outDir>"
```

Pass a different game path as a 3rd/4th argument if it is not at the default Steam location. The tool
skips localized files (`*.xx-XX.xnb`) and writes one pretty JSON per asset.

## What is collected for Linus (`data/linus/`)

`linus-canon.md` is the consolidated, human-readable reference (with a markup legend). The raw JSON lives in `raw/`:

| File | Source | Count |
|---|---|---|
| `raw/Linus.json` | `Characters/Dialogue/Linus` | 53 dialogue entries |
| `raw/rainy_Linus.json` | `Characters/Dialogue/rainy` (Linus key) | 1 line |
| `raw/festivals_Linus.json` | `Data/Festivals/*` (Linus keys) | 16 lines |
| `raw/events_Linus.json` | `Data/Events/*` (Linus `speak` lines) | 29 lines across 7 events |

Dialogue markup (`#$e#`, `$h`, `@`, `#$1 flag#`, `$y` question branches, item refs) is kept **verbatim**;
parsing and cleaning it is a step in the dataset-authoring plan, not the collection.

## Note on game text and licensing

This is ConcernedApe's copyrighted dialogue, used to train a derived model for a free, non-commercial mod
(the same basis every AI-dialogue Stardew mod stands on). We do not redistribute the raw game files; the
trained adapter is the derived artifact. Keep it free and clearly not canon.

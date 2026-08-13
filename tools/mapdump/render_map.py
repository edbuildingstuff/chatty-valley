"""Render a Stardew Valley location from the installed game's own map and tilesheets.

Authoring a villager's rows means claiming things about the room they stand in. The setting doc for
Elliott listed a piano, a rose and a desk, and sixteen authored rows still leaned on a fireplace and
a stove that his cabin does not have. This renders the room so the inventory is observed rather than
remembered, and it prints the tile Actions, which carry the game's own examine strings.

Needs the game installed and `dotnet` on PATH. Build the two helpers once:
    dotnet build tools/mapdump/unxnb.csproj
    dotnet build tools/mapdump/readmap.csproj

Then:
    python tools/mapdump/render_map.py ElliottHouse
    python tools/mapdump/render_map.py Saloon --out docs/media/saloon.png

Reads only. Nothing is written into the game folder.
"""
import argparse, os, struct, subprocess, sys, tempfile
from PIL import Image

GAME = os.environ.get(
    "STARDEW_PATH",
    r"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley")
MAPS = os.path.join(GAME, "Content", "Maps")
HERE = os.path.dirname(os.path.abspath(__file__))
UNXNB = os.path.join(HERE, "bin", "unxnb", "unxnb.dll")
READMAP = os.path.join(HERE, "bin", "readmap", "readmap.dll")


def build_if_missing():
    for proj, out in (("unxnb.csproj", UNXNB), ("readmap.csproj", READMAP)):
        if not os.path.exists(out):
            subprocess.run(["dotnet", "build", os.path.join(HERE, proj),
                            "-o", os.path.dirname(out), "-v", "q", "--nologo"], check=True)


def unpack(name, work):
    raw = os.path.join(work, name + ".raw")
    subprocess.run(["dotnet", UNXNB, os.path.join(MAPS, name + ".xnb"), raw],
                   check=True, stdout=subprocess.DEVNULL)
    return raw


def strip_xnb_prefix(raw):
    d = open(raw, "rb").read()
    out = raw + ".tbin"
    open(out, "wb").write(d[d.index(b"tBIN10"):])
    return out


def read_texture(path):
    """XNB Texture2D body: reader table, then surface format, size, mip data."""
    d = open(path, "rb").read()
    o = 0

    def s7(o):
        v = sh = 0
        while True:
            b = d[o]; o += 1
            v |= (b & 0x7F) << sh
            if not b & 0x80:
                return v, o
            sh += 7

    n, o = s7(o)
    for _ in range(n):
        ln, o = s7(o)
        o += ln + 4
    _, o = s7(o)   # shared resource count
    _, o = s7(o)   # type id
    fmt, w, h, _mips = struct.unpack_from("<iiii", d, o); o += 16
    size = struct.unpack_from("<i", d, o)[0]; o += 4
    if fmt != 0:
        raise SystemExit(f"{path}: surface format {fmt} is not uncompressed Color")
    return Image.frombytes("RGBA", (w, h), d[o:o + size])


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("location", help="map name, e.g. ElliottHouse, Saloon, LeahHouse")
    ap.add_argument("--out", help="output PNG (default <location>.png in the cwd)")
    ap.add_argument("--scale", type=int, default=0, help="pixel scale, 0 picks a sensible one")
    args = ap.parse_args()

    if not os.path.isdir(MAPS):
        raise SystemExit(f"no game maps at {MAPS}; set STARDEW_PATH")
    build_if_missing()

    with tempfile.TemporaryDirectory() as work:
        tbin = strip_xnb_prefix(unpack(args.location, work))
        grid = os.path.join(work, "grid.tsv")
        report = subprocess.run(["dotnet", READMAP, tbin, grid],
                                check=True, capture_output=True, text=True).stdout
        print(report)

        sheets, layers, cur = {}, [], None
        for line in open(grid, encoding="utf-8"):
            line = line.rstrip("\n")
            if line.startswith("#SHEET"):
                _, sid, img, _sw, _sh, tw, th = line.split("\t")
                sheets[sid] = {"image": os.path.basename(img), "tile": (int(tw), int(th))}
            elif line.startswith("#LAYER"):
                _, name, w, h = line.split("\t")
                cur = {"name": name, "w": int(w), "h": int(h), "rows": []}
                layers.append(cur)
            elif line:
                cur["rows"].append(line.split("\t"))

        for sid, s in sheets.items():
            s["img"] = read_texture(unpack(s["image"], work))
            s["cols"] = s["img"].width // s["tile"][0]

        TS = 16
        W, H = layers[0]["w"] * TS, layers[0]["h"] * TS
        canvas = Image.new("RGBA", (W, H), (0, 0, 0, 255))
        for layer in layers:
            if layer["name"] == "Paths":      # pathing hints, never drawn in game
                continue
            for y, row in enumerate(layer["rows"]):
                for x, cell in enumerate(row):
                    if cell == "-1":
                        continue
                    sid, idx = cell.rsplit(":", 1)
                    s = sheets[sid]
                    tw, th = s["tile"]
                    sx, sy = (int(idx) % s["cols"]) * tw, (int(idx) // s["cols"]) * th
                    canvas.alpha_composite(s["img"].crop((sx, sy, sx + tw, sy + th)), (x * TS, y * TS))

        scale = args.scale or max(1, min(5, 1600 // W))
        out = args.out or (args.location + ".png")
        canvas.resize((W * scale, H * scale), Image.NEAREST).save(out)
        print(f"wrote {out} ({W * scale}x{H * scale}, {scale}x)")


if __name__ == "__main__":
    main()

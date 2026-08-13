using System;
using System.IO;
using System.Linq;
using xTile;
using xTile.Format;
using xTile.Layers;
using xTile.Tiles;

class Program
{
    static int Main(string[] args)
    {
        using var fs = File.OpenRead(args[0]);
        var fmt = FormatManager.Instance.BinaryFormat;
        Map map = fmt.Load(fs);
        Console.WriteLine($"MAP {map.Id}  props: " +
            string.Join(", ", map.Properties.Select(kv => $"{kv.Key}={kv.Value}")));
        foreach (var ts in map.TileSheets)
            Console.WriteLine($"SHEET {ts.Id} image={ts.ImageSource} sheetSize={ts.SheetSize} tileSize={ts.TileSize}");
        foreach (Layer layer in map.Layers)
        {
            int count = 0, anim = 0;
            for (int y = 0; y < layer.LayerHeight; y++)
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    Tile t = layer.Tiles[x, y];
                    if (t == null) continue;
                    count++;
                    if (t is AnimatedTile at)
                    {
                        anim++;
                        Console.WriteLine($"  ANIM {layer.Id} ({x},{y}) interval={at.FrameInterval} " +
                            $"frames=[{string.Join(",", at.TileFrames.Select(f => f.TileIndex))}] sheet={at.TileSheet.Id}");
                    }
                    if (t.Properties.Count > 0)
                        Console.WriteLine($"  PROP {layer.Id} ({x},{y}) idx={t.TileIndex} " +
                            string.Join("; ", t.Properties.Select(kv => $"{kv.Key}={kv.Value}")));
                }
            Console.WriteLine($"LAYER {layer.Id} {layer.LayerWidth}x{layer.LayerHeight} tiles={count} animated={anim}");
        }
        // dump the tile index grid per layer for rendering
        using var w = new StreamWriter(args[1]);
        foreach (var ts in map.TileSheets)
            w.WriteLine($"#SHEET	{ts.Id}	{ts.ImageSource}	{ts.SheetSize.Width}	{ts.SheetSize.Height}	{ts.TileSize.Width}	{ts.TileSize.Height}");
        foreach (Layer layer in map.Layers)
        {
            w.WriteLine($"#LAYER\t{layer.Id}\t{layer.LayerWidth}\t{layer.LayerHeight}");
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                var cells = new string[layer.LayerWidth];
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    Tile t = layer.Tiles[x, y];
                    cells[x] = t == null ? "-1" : (t.TileSheet.Id + ":" + t.TileIndex);
                }
                w.WriteLine(string.Join("\t", cells));
            }
        }
        return 0;
    }
}

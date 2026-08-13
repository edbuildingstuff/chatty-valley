using System;
using System.IO;
using System.Reflection;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2) { Console.Error.WriteLine("usage: unxnb <in.xnb> <out.bin>"); return 2; }
        byte[] d = File.ReadAllBytes(args[0]);
        if (d[0] != 'X' || d[1] != 'N' || d[2] != 'B') { Console.Error.WriteLine("not XNB"); return 3; }
        byte flags = d[5];
        bool compressed = (flags & 0x80) != 0;
        if (!compressed)
        {
            File.WriteAllBytes(args[1], d[10..]);
            Console.WriteLine($"uncompressed, wrote {d.Length - 10} bytes");
            return 0;
        }
        int declared = BitConverter.ToInt32(d, 10);

        Assembly asm = typeof(Microsoft.Xna.Framework.Vector2).Assembly;
        Type lzx = null;
        foreach (var t in asm.GetTypes()) if (t.Name == "LzxDecoder") { lzx = t; break; }
        if (lzx == null) { Console.Error.WriteLine("LzxDecoder not found"); return 4; }

        var ctor = lzx.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                      null, new[] { typeof(int) }, null);
        object dec = ctor.Invoke(new object[] { 16 });
        MethodInfo decompress = lzx.GetMethod("Decompress",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var inMs = new MemoryStream(d, 14, d.Length - 14);
        var outMs = new MemoryStream(declared);
        long pos = 0, fileLen = inMs.Length;
        while (pos < fileLen)
        {
            int hi = inMs.ReadByte();
            if (hi < 0) break;
            int lo = inMs.ReadByte();
            if (lo < 0) break;
            int blockSize, frameSize;
            if (hi == 0xFF)
            {
                int h2 = lo, l2 = inMs.ReadByte();
                frameSize = (h2 << 8) | l2;
                int h3 = inMs.ReadByte(), l3 = inMs.ReadByte();
                blockSize = (h3 << 8) | l3;
                pos += 5;
            }
            else
            {
                blockSize = (hi << 8) | lo;
                frameSize = 0x8000;
                pos += 2;
            }
            if (blockSize == 0 || frameSize == 0) break;
            decompress.Invoke(dec, new object[] { inMs, blockSize, outMs, frameSize });
            pos += blockSize;
            inMs.Position = pos;
        }
        byte[] outBytes = outMs.ToArray();
        File.WriteAllBytes(args[1], outBytes);
        Console.WriteLine($"declared={declared} wrote={outBytes.Length}");
        return 0;
    }
}

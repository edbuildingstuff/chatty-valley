using System.Runtime.Loader;
using System.Text.RegularExpressions;

// Usage:
//   DialogueDump <assetName> <outFile.json> [gameDir]      dump one asset (e.g. Characters\Dialogue\Linus)
//   DialogueDump --dir <contentRelDir> <outDir> [gameDir]  dump every English .xnb in Content\<dir>
//
// The assembly resolver must be registered before any MonoGame type is touched, so the real work lives
// in Runner (a separate type only JIT-loaded when called).

bool dirMode = args.Length > 0 && args[0] == "--dir";
string gameDir = args.Length > 3
    ? args[3]
    : @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley";

AssemblyLoadContext.Default.Resolving += (ctx, name) =>
{
    string path = Path.Combine(gameDir, name.Name + ".dll");
    return File.Exists(path) ? ctx.LoadFromAssemblyPath(path) : null;
};

try
{
    if (dirMode)
    {
        string relDir = args[1];
        string outDir = args[2];
        var locale = new Regex(@"\.[a-z]{2}-[A-Z]{2}$");
        var files = Directory.GetFiles(Path.Combine(gameDir, "Content", relDir), "*.xnb")
            .Where(f => !locale.IsMatch(Path.GetFileNameWithoutExtension(f)))
            .OrderBy(f => f);
        int total = 0;
        foreach (var f in files)
        {
            string name = Path.GetFileNameWithoutExtension(f);
            try
            {
                int n = ChattyValley.DialogueDump.Runner.Dump(gameDir, Path.Combine(relDir, name), Path.Combine(outDir, name + ".json"));
                Console.WriteLine($"{name,-24} {n,4} entries");
                total++;
            }
            catch (Exception ex) { Console.Error.WriteLine($"skip {name}: {ex.Message}"); }
        }
        Console.WriteLine($"dumped {total} files -> {outDir}");
        return 0;
    }
    else
    {
        string asset = args.Length > 0 ? args[0] : @"Characters\Dialogue\Linus";
        string outPath = args.Length > 1 ? args[1] : "out.json";
        int n = ChattyValley.DialogueDump.Runner.Dump(gameDir, asset, outPath);
        Console.WriteLine($"{asset}: {n} entries -> {outPath}");
        return 0;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAILED: {ex.GetType().Name}: {ex.Message}");
    return 1;
}

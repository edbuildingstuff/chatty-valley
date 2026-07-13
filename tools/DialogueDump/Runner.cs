using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ChattyValley.DialogueDump;

/// <summary>
/// Kept as a separate type so MonoGame.Framework is not loaded until after Program has registered the
/// assembly resolver. Uses the game's ContentManager headlessly to load a Dictionary&lt;string,string&gt;.
/// </summary>
public static class Runner
{
    public static int Dump(string gameDir, string asset, string outPath)
    {
        using var content = new ContentManager(new GameServiceContainer(), Path.Combine(gameDir, "Content"));
        var dict = content.Load<Dictionary<string, string>>(asset);

        string? dir = Path.GetDirectoryName(Path.GetFullPath(outPath));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(outPath, JsonSerializer.Serialize(dict, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }));
        return dict.Count;
    }
}

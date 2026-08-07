using ChattyValley.Core;

namespace ChattyValley.Tests;

public class CharacterRosterTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("cv-roster-tests").FullName;
    private string CharsDir => Path.Combine(_root, "characters");
    public CharacterRosterTests() => Directory.CreateDirectory(CharsDir);
    public void Dispose() => Directory.Delete(_root, recursive: true);

    private void Write(string file, string json) =>
        File.WriteAllText(Path.Combine(CharsDir, file), json);

    [Fact]
    public void LoadsCharactersAndResolvesRelativeAdapterPathsAgainstBaseDir()
    {
        Write("linus.json", "{\"name\":\"Linus\",\"bio\":\"b\",\"adapterPath\":\"assets/linus.gguf\"}");
        Write("pierre.json", "{\"name\":\"Pierre\",\"bio\":\"b\",\"adapterPath\":\"assets/pierre.gguf\"}");
        var r = CharacterRoster.Load(CharsDir, _root);
        Assert.Equal(2, r.Entries.Count);
        Assert.Equal(Path.Combine(_root, "assets", "linus.gguf"), r.Entries["Linus"].AdapterFullPath);
        Assert.Empty(r.Warnings);
    }

    [Fact]
    public void AbsoluteAdapterPath_IsKeptAsIs()
    {
        string abs = Path.Combine(_root, "elsewhere", "linus.gguf");
        Write("linus.json", $"{{\"name\":\"Linus\",\"bio\":\"b\",\"adapterPath\":{System.Text.Json.JsonSerializer.Serialize(abs)}}}");
        var r = CharacterRoster.Load(CharsDir, _root);
        Assert.Equal(abs, r.Entries["Linus"].AdapterFullPath);
    }

    [Fact]
    public void NullAdapterPath_IsSkippedWithWarning()
    {
        // Per the spec: no prompt-only fallback in the mod. A character without an adapter is
        // not chattable and says so once in the log.
        Write("linus.json", "{\"name\":\"Linus\",\"bio\":\"b\",\"adapterPath\":null}");
        var r = CharacterRoster.Load(CharsDir, _root);
        Assert.Empty(r.Entries);
        Assert.Single(r.Warnings);
        Assert.Contains("linus.json", r.Warnings[0]);
        Assert.Contains("no adapterPath", r.Warnings[0]);
    }

    [Fact]
    public void BadJson_IsSkippedWithWarning_AndOthersStillLoad()
    {
        Write("broken.json", "{not json");
        Write("linus.json", "{\"name\":\"Linus\",\"bio\":\"b\",\"adapterPath\":\"a.gguf\"}");
        var r = CharacterRoster.Load(CharsDir, _root);
        Assert.Single(r.Entries);
        Assert.Single(r.Warnings);
        Assert.Contains("broken.json", r.Warnings[0]);
    }

    [Fact]
    public void MissingName_IsSkippedWithWarning()
    {
        Write("noname.json", "{\"bio\":\"b\",\"adapterPath\":\"a.gguf\"}");
        var r = CharacterRoster.Load(CharsDir, _root);
        Assert.Empty(r.Entries);
        Assert.Single(r.Warnings);
    }

    [Fact]
    public void DuplicateName_FirstFileWins_WithWarning()
    {
        Write("a-linus.json", "{\"name\":\"Linus\",\"bio\":\"first\",\"adapterPath\":\"a.gguf\"}");
        Write("b-linus.json", "{\"name\":\"Linus\",\"bio\":\"second\",\"adapterPath\":\"b.gguf\"}");
        var r = CharacterRoster.Load(CharsDir, _root);
        Assert.Single(r.Entries);
        Assert.Equal("first", r.Entries["Linus"].Character.Bio);
        Assert.Single(r.Warnings);
        Assert.Contains("b-linus.json", r.Warnings[0]);
    }

    [Fact]
    public void MissingDirectory_ReturnsEmptyWithWarning()
    {
        var r = CharacterRoster.Load(Path.Combine(_root, "nope"), _root);
        Assert.Empty(r.Entries);
        Assert.Single(r.Warnings);
    }
}

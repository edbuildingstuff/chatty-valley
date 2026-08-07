using ChattyValley.Core;

namespace ChattyValley.Tests;

public class AdapterValidationTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("cv-adapter-tests").FullName;
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private string Write(string file, byte[] bytes)
    {
        string p = Path.Combine(_dir, file);
        File.WriteAllBytes(p, bytes);
        return p;
    }

    [Fact]
    public void MissingFile_ReportsMissingFile()
    {
        var s = AdapterValidation.Check("Linus", Path.Combine(_dir, "nope.gguf"));
        Assert.Equal(AdapterStatuses.MissingFile, s.Status);
        Assert.Equal("Linus", s.Name);
    }

    [Fact]
    public void WrongMagic_ReportsNotGguf()
    {
        string p = Write("bad.gguf", new byte[] { 0x50, 0x4B, 0x03, 0x04, 0, 0, 0, 0 }); // zip magic
        Assert.Equal(AdapterStatuses.NotGguf, AdapterValidation.Check("Linus", p).Status);
    }

    [Fact]
    public void TruncatedFile_ReportsNotGguf()
    {
        string p = Write("short.gguf", new byte[] { 0x47, 0x47 }); // only "GG"
        Assert.Equal(AdapterStatuses.NotGguf, AdapterValidation.Check("Linus", p).Status);
    }

    [Fact]
    public void GgufMagic_ReportsOk()
    {
        // "GGUF" ASCII magic plus a few payload bytes.
        string p = Write("good.gguf", new byte[] { 0x47, 0x47, 0x55, 0x46, 3, 0, 0, 0 });
        var s = AdapterValidation.Check("Linus", p);
        Assert.Equal(AdapterStatuses.Ok, s.Status);
        Assert.Null(s.Detail);
    }
}

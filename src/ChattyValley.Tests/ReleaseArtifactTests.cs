using System.Text.Json;

namespace ChattyValley.Tests;

/// <summary>
/// The shipped config and manifest are release-critical and reach every first-time installer, but
/// ModConfig itself lives behind SMAPI and cannot be referenced here. So the release values live in
/// a checked-in artifact and are asserted as data.
/// </summary>
public class ReleaseArtifactTests
{
    private static JsonElement Load(string file)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "packaging", file);
        return JsonDocument.Parse(File.ReadAllText(path)).RootElement;
    }

    [Fact]
    public void ReleaseConfig_UsesTheValidatedDecodeRegime()
    {
        // 0.6 is the regime where false-premise adoption measurably worsens.
        Assert.Equal(0.35, Load("config.release.json").GetProperty("Temperature").GetDouble(), 3);
    }

    [Fact]
    public void ReleaseConfig_DisablesDevelopmentChatLogging()
    {
        Assert.False(Load("config.release.json").GetProperty("ChatLogEnabled").GetBoolean());
    }

    [Fact]
    public void ReleaseConfig_LeavesModelPathsBlankSoTheyResolveInsideTheModFolder()
    {
        var cfg = Load("config.release.json");
        Assert.Equal("", cfg.GetProperty("BaseModelPath").GetString());
        Assert.Equal("", cfg.GetProperty("LinusAdapterPath").GetString());
    }

    [Fact]
    public void ReleaseConfig_KeepsTheFalsePremiseGuardEnabled()
    {
        Assert.True(Load("config.release.json").GetProperty("FalsePremiseGuard").GetBoolean());
    }

    [Fact]
    public void ReleaseConfig_RunsOnCpuByDefault()
    {
        Assert.Equal(0, Load("config.release.json").GetProperty("GpuLayers").GetInt32());
    }

    [Fact]
    public void Manifest_DeclaresTheEarlyAccessReleaseVersion()
    {
        Assert.Equal("0.2.0", Load("manifest.json").GetProperty("Version").GetString());
    }
}

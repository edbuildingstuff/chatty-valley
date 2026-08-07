using System.Text.Json;
using System.Text.RegularExpressions;

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
        Assert.Equal("0.2.1", Load("manifest.json").GetProperty("Version").GetString());
    }

    /// <summary>
    /// Every ModConfig property must appear in the shipped config, and vice versa. The spot-check
    /// below covers two values; it cannot catch a whole key going missing, which is how 0.2.1
    /// shipped without FalsePremiseGuardClause. SMAPI heals that on first load, so the only player
    /// who sees it is the one reading config.json before they have launched once, which is exactly
    /// the player README.txt sends there.
    /// </summary>
    [Fact]
    public void ReleaseConfig_CarriesEveryModConfigPropertyAndNoOthers()
    {
        string source = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "packaging", "ModConfig.cs"));
        string[] declared = Regex
            .Matches(source, @"public\s+[\w<>?\[\]]+\s+(\w+)\s*\{\s*get;\s*set;\s*\}")
            .Select(m => m.Groups[1].Value)
            .ToArray();

        // Guards the regex itself: a silently-matching-nothing pattern would make this test vacuous.
        Assert.NotEmpty(declared);

        JsonElement cfg = Load("config.release.json");
        string[] shipped = cfg.EnumerateObject().Select(p => p.Name).ToArray();

        string[] missing = declared.Except(shipped).ToArray();
        string[] stale = shipped.Except(declared).ToArray();

        Assert.True(
            missing.Length == 0,
            $"config.release.json is missing ModConfig properties: {string.Join(", ", missing)}");
        Assert.True(
            stale.Length == 0,
            $"config.release.json carries keys ModConfig no longer declares: {string.Join(", ", stale)}");
    }

    [Fact]
    public void ModConfigDefaults_AgreeWithTheReleaseConfig()
    {
        // ModConfig cannot be referenced here (it depends on SMAPI), but a drifted default still
        // reaches any player who deletes config.json, so assert against the source text.
        string modConfig = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "packaging", "ModConfig.cs"));

        Assert.Contains("Temperature { get; set; } = 0.35f;", modConfig);
        Assert.Contains("ChatLogEnabled { get; set; } = false;", modConfig);
    }

    [Fact]
    public void ModEntryFallbacks_PointAtTheShippingModelPair()
    {
        string modEntry = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "packaging", "ModEntry.cs"));

        Assert.Contains("LFM2.5-1.2B-Instruct-Q4_K_M.gguf", modEntry);
        Assert.Contains("linus-12b-v8dpo2-lora-f16.gguf", modEntry);
        Assert.DoesNotContain("350M", modEntry);
        Assert.DoesNotContain("350m", modEntry);
    }
}

using StardewModdingAPI;

namespace ChattyValley.Mod;

/// <summary>Player-editable config (config.json, written by SMAPI on first run).</summary>
public sealed class ModConfig
{
    /// <summary>Key that opens free-chat with the villager you are facing (when it is safe to).</summary>
    public SButton ChatKey { get; set; } = SButton.C;

    /// <summary>After a villager's canonical dialogue closes, surface a small "keep talking" hint.</summary>
    public bool ShowContinuationHint { get; set; } = true;

    /// <summary>Sampling temperature and reply cap for the local model.</summary>
    public float Temperature { get; set; } = 0.6f;
    public int MaxTokens { get; set; } = 96;

    /// <summary>
    /// Absolute paths to the GGUF files. Left blank = resolve from the repo's models/ folder (dev) and
    /// the trained adapter. Set explicitly for a deployed install. The adapter path can point anywhere.
    /// </summary>
    public string BaseModelPath { get; set; } = "";
    public string LinusAdapterPath { get; set; } = "";

    /// <summary>GPU layers to offload (0 = CPU only, the shipping default so it runs on any machine).</summary>
    public int GpuLayers { get; set; } = 0;
}

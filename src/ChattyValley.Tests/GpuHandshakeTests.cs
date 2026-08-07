using System.Text.Json;
using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The gpu handshake object is the only channel telling the mod (and the player, via the status
/// line) whether inference runs on GPU. Old sidecars send no gpu field at all, so absence must
/// deserialize as null rather than throwing.
/// </summary>
public class GpuHandshakeTests
{
    [Fact]
    public void GpuStatus_RoundTripsThroughSidecarJson()
    {
        var hs = new SidecarHandshake(true, "base.gguf", new[] { new AdapterStatus("Linus", AdapterStatuses.Ok) },
            new GpuStatus(GpuModes.Auto, true, "NVIDIA GeForce RTX 2070 with Max-Q Design"));
        string json = JsonSerializer.Serialize(hs, SidecarJson.Options);
        var back = JsonSerializer.Deserialize<SidecarHandshake>(json, SidecarJson.Options)!;
        Assert.NotNull(back.Gpu);
        Assert.Equal(GpuModes.Auto, back.Gpu!.Requested);
        Assert.True(back.Gpu.Active);
        Assert.Equal("NVIDIA GeForce RTX 2070 with Max-Q Design", back.Gpu.Device);
        Assert.Null(back.Gpu.FallbackReason);
    }

    [Fact]
    public void Handshake_WithoutGpuField_DeserializesWithNullGpu()
    {
        string legacy = "{\"ready\":true,\"base\":\"base.gguf\",\"adapters\":[]}";
        var hs = JsonSerializer.Deserialize<SidecarHandshake>(legacy, SidecarJson.Options)!;
        Assert.True(hs.Ready);
        Assert.Null(hs.Gpu);
    }

    [Fact]
    public void GpuStatus_SerializesCamelCase()
    {
        string json = JsonSerializer.Serialize(
            new GpuStatus(GpuModes.On, false, null, "vk init failed"), SidecarJson.Options);
        Assert.Contains("\"requested\":\"on\"", json);
        Assert.Contains("\"fallbackReason\":\"vk init failed\"", json);
    }
}

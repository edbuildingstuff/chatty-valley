using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>One console line tells the player where replies run. GPU wording only, never Vulkan.</summary>
public class GpuStatusMessagesTests
{
    [Fact]
    public void ActiveGpu_NamesTheDevice()
    {
        var (text, warn) = GpuStatusMessages.Build(new GpuStatus(GpuModes.Auto, true, "NVIDIA GeForce RTX 2070"));
        Assert.Equal("AI replies: GPU (NVIDIA GeForce RTX 2070)", text);
        Assert.False(warn);
    }

    [Fact]
    public void CpuByResolution_SaysCpuPlainly()
    {
        var (text, warn) = GpuStatusMessages.Build(new GpuStatus(GpuModes.Auto, false));
        Assert.Equal("AI replies: CPU", text);
        Assert.False(warn);
    }

    [Fact]
    public void FallbackAfterGpuFailure_WarnsWithTheReason()
    {
        var (text, warn) = GpuStatusMessages.Build(new GpuStatus(GpuModes.On, false, null, "vk device lost"));
        Assert.Equal("AI replies: CPU (GPU failed to start and was skipped: vk device lost)", text);
        Assert.True(warn);
    }

    [Fact]
    public void OldSidecarWithoutGpuField_SaysCpu()
    {
        var (text, warn) = GpuStatusMessages.Build(null);
        Assert.Equal("AI replies: CPU", text);
        Assert.False(warn);
    }

    [Fact]
    public void NoPlayerFacingLine_EverSaysVulkan()
    {
        foreach (var gpu in new[]
        {
            new GpuStatus(GpuModes.Auto, true, "AMD Radeon RX 6700"),
            new GpuStatus(GpuModes.On, false, null, "boom"),
            new GpuStatus(GpuModes.Off, false),
        })
            Assert.DoesNotContain("vulkan", GpuStatusMessages.Build(gpu).Text, StringComparison.OrdinalIgnoreCase);
    }
}

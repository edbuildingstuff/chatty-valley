using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The ship default is GPU-on for dedicated NVIDIA/AMD GPUs only: the DAT-703 iGPU arm measured
/// the Intel UHD 630 at 2.8x SLOWER than CPU, so integrated GPUs must classify ineligible.
/// Vendor comes from PNPDeviceID (VEN_10DE NVIDIA, VEN_1002 AMD); the >=2GB floor filters iGPU
/// carve-outs and tiny cards. WMI's AdapterRAM is uint32-capped (an 8GB card reports ~4GB),
/// which a floor comparison tolerates by construction.
/// </summary>
public class GpuEligibilityTests
{
    private static GpuAdapterInfo Nvidia(long ram) =>
        new(@"PCI\VEN_10DE&DEV_1F10&SUBSYS_09161028", "NVIDIA GeForce RTX 2070 with Max-Q Design", ram);
    private static GpuAdapterInfo Amd(long ram) =>
        new(@"PCI\VEN_1002&DEV_731F", "AMD Radeon RX 5700 XT", ram);
    private static GpuAdapterInfo IntelIgpu() =>
        new(@"PCI\VEN_8086&DEV_3E9B", "Intel(R) UHD Graphics 630", 1L * 1024 * 1024 * 1024);

    private const long TwoGb = 2L * 1024 * 1024 * 1024;

    [Fact] public void NvidiaDgpu_IsEligible() => Assert.True(GpuEligibility.IsEligible(Nvidia(4293918720)));
    [Fact] public void AmdDgpu_AtExactlyTwoGb_IsEligible() => Assert.True(GpuEligibility.IsEligible(Amd(TwoGb)));
    [Fact] public void IntelIgpu_IsNotEligible() => Assert.False(GpuEligibility.IsEligible(IntelIgpu()));
    [Fact] public void AmdApuCarveOut_UnderTwoGb_IsNotEligible() => Assert.False(GpuEligibility.IsEligible(Amd(1L * 1024 * 1024 * 1024)));
    [Fact] public void NullPnpId_IsNotEligible() => Assert.False(GpuEligibility.IsEligible(new GpuAdapterInfo(null, "Mystery", TwoGb)));
    [Fact] public void VendorMatch_IsCaseInsensitive() =>
        Assert.True(GpuEligibility.IsEligible(new GpuAdapterInfo(@"pci\ven_10de&dev_1f10", "NVIDIA", TwoGb)));

    [Fact]
    public void PickEligible_PrefersTheDgpuOnADualDeviceMachine()
    {
        var picked = GpuEligibility.PickEligible(new[] { IntelIgpu(), Nvidia(4293918720) });
        Assert.NotNull(picked);
        Assert.Contains("NVIDIA", picked!.Name);
    }

    [Fact] public void PickEligible_ReturnsNullWhenNothingQualifies() =>
        Assert.Null(GpuEligibility.PickEligible(new[] { IntelIgpu() }));
    [Fact] public void PickEligible_ReturnsNullOnEmptyList() =>
        Assert.Null(GpuEligibility.PickEligible(Array.Empty<GpuAdapterInfo>()));
}

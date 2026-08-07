namespace ChattyValley.Core;

/// <summary>One display adapter as reported by the OS (the sidecar fills this from WMI).</summary>
public sealed record GpuAdapterInfo(string? PnpDeviceId, string? Name, long AdapterRamBytes);

/// <summary>
/// Decides whether a machine gets GPU inference under "Gpu": "auto". Dedicated NVIDIA/AMD only:
/// the DAT-703 iGPU arm measured the Intel UHD 630 at 2.8x slower than CPU, so integrated GPUs
/// stay on the proven CPU path. Pure function so the matrix is unit-testable; the WMI query that
/// feeds it lives in the sidecar. Known-accepted imperfections: WMI AdapterRAM is uint32-capped
/// (harmless under a floor comparison) and a large AMD APU carve-out can pass (runs slower-but-
/// correct, and the config override exists).
/// </summary>
public static class GpuEligibility
{
    private const long MinAdapterRamBytes = 2L * 1024 * 1024 * 1024;

    public static bool IsEligible(GpuAdapterInfo a)
    {
        if (a.PnpDeviceId is null) return false;
        bool dedicatedVendor =
            a.PnpDeviceId.Contains("VEN_10DE", StringComparison.OrdinalIgnoreCase) ||   // NVIDIA
            a.PnpDeviceId.Contains("VEN_1002", StringComparison.OrdinalIgnoreCase);     // AMD
        return dedicatedVendor && a.AdapterRamBytes >= MinAdapterRamBytes;
    }

    public static GpuAdapterInfo? PickEligible(IEnumerable<GpuAdapterInfo> adapters)
        => adapters.FirstOrDefault(IsEligible);
}

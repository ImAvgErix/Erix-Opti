using ErixOpti.Core.Models;
using ErixOpti.Core.Services.Tweaks;

namespace ErixOpti.Core.Services;

public static class TweakCatalog
{
    public static IReadOnlyList<TweakOperation> All(HardwareInfo? hw)
    {
        HwRef.Hw = hw;
        return
        [
            ..RegistryTweaks.All,
            ..ServiceTweaks.All,
            ..PowerTweaks.All,
            ..GpuTweaks.All,
            ..NetworkTweaks.All,
            ..ExtremeTweaks.All,
            ..CleanupTweaks.All,
        ];
    }
}

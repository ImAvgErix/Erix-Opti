using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class GpuTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "gpu.mpo-off", Name = "MPO off (multi-monitor)", Category = "GPU", ShouldApply = hw => hw.HasMultiMonitor,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", out var v) ? v == 5 : false,
            Apply = (p, _) => { p.Report("MPO off"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 5); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode"); return Task.CompletedTask; } },
        new() { Id = "gpu.hags", Name = "HAGS config", Category = "GPU", ShouldApply = _ => true,
            TryGetAppliedState = hw =>
            {
                if (!RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", out var cur))
                    return false;
                var want = hw.HasLowVram ? 1 : 2;
                return cur == want;
            },
            Apply = (p, _) => { var v = HwRef.Hw?.HasLowVram == true ? 1 : 2; p.Report($"HAGS={v}"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", v); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2); return Task.CompletedTask; } },
        new() { Id = "gpu.tdr-delay", Name = "TDR delay (multi-monitor)", Category = "GPU", ShouldApply = hw => hw.HasMultiMonitor,
            TryGetAppliedState = _ =>
            {
                var a = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay", out var d) && d == 10;
                var b = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDdiDelay", out var e) && e == 10;
                return a && b;
            },
            Apply = (p, _) => { p.Report("TdrDelay=10"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay", 10); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDdiDelay", 10); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay"); RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDdiDelay"); return Task.CompletedTask; } },
        new() { Id = "gpu.nv-shader", Name = "NVIDIA shader cache", Category = "GPU", ShouldApply = hw => hw.IsNvidiaGpu,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(RegistryHive.CurrentUser, @"Software\NVIDIA Corporation\Global\NVTweak", "Gestalt", out var g) && g == 1,
            Apply = (p, _) => { p.Report("NVIDIA shader pre-cache"); RegistryTweakHelper.WriteDword(RegistryHive.CurrentUser, @"Software\NVIDIA Corporation\Global\NVTweak", "Gestalt", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\NVIDIA Corporation\Global\NVTweak", "Gestalt"); return Task.CompletedTask; } },
        new() { Id = "gpu.nv-pstate", Name = "NVIDIA P-State 0 (max perf)", Category = "GPU", ShouldApply = hw => hw.IsNvidiaGpu,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "DisableDynamicPstate", out var p) ? p == 1 : null,
            Apply = (p, _) => { p.Report("NVIDIA prefer max performance"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "DisableDynamicPstate", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "DisableDynamicPstate"); return Task.CompletedTask; } },
        new() { Id = "gpu.dwm-vblank", Name = "DWM VBlank optimized", Category = "GPU", ShouldApply = _ => true,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "DisableFlipWaitOverride", out var x) && x == 1,
            Apply = (p, _) => { p.Report("DWM VBlank disable wait"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "DisableFlipWaitOverride", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "DisableFlipWaitOverride"); return Task.CompletedTask; } },
        new() { Id = "gpu.game-sched", Name = "GPU game scheduling priority", Category = "GPU", ShouldApply = _ => true,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", out var g) && g == 8,
            Apply = (p, _) => { p.Report("GPU scheduling priority 8"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 0); return Task.CompletedTask; } },
    ];
}

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class GpuTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "gpu.mpo-off", Name = "MPO off", Category = "GPU",
            Description = "Disables Multi-Plane Overlay (fixes flickering on multi-monitor).",
            ShouldApply = hw => hw.HasMultiMonitor,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", out var v) && v == 5,
            Apply = (p, _) => { p.Report("MPO off"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 5); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode"); return Task.CompletedTask; } },
        new() { Id = "gpu.hags", Name = "HAGS config", Category = "GPU",
            Description = "Hardware-accelerated GPU scheduling: on if VRAM>8GB, off if ≤8GB.",
            ShouldApply = _ => true,
            TryGetAppliedState = hw => { if (!RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", out var c)) return false; return c == (hw.HasLowVram ? 1 : 2); },
            Apply = (p, _) => { var v = HwRef.Hw?.HasLowVram == true ? 1 : 2; p.Report($"HAGS={v}"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", v); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2); return Task.CompletedTask; } },
        new() { Id = "gpu.tdr-delay", Name = "TDR delay 10s", Category = "GPU",
            Description = "Increases TDR timeout to prevent false GPU resets (multi-monitor).",
            ShouldApply = hw => hw.HasMultiMonitor,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay", out var d) && d == 10 && RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDdiDelay", out var e) && e == 10,
            Apply = (p, _) => { p.Report("TDR delay=10"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay", 10); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDdiDelay", 10); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay"); RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDdiDelay"); return Task.CompletedTask; } },
        new() { Id = "gpu.nv-shader", Name = "NVIDIA shader cache", Category = "GPU",
            Description = "Enables NVIDIA shader pre-cache for faster game loads.",
            ShouldApply = hw => hw.IsNvidiaGpu,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.CurrentUser, @"Software\NVIDIA Corporation\Global\NVTweak", "Gestalt", out var g) && g == 1,
            Apply = (p, _) => { p.Report("NVIDIA shader cache"); RegistryTweakHelper.WriteDword(RegistryHive.CurrentUser, @"Software\NVIDIA Corporation\Global\NVTweak", "Gestalt", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\NVIDIA Corporation\Global\NVTweak", "Gestalt"); return Task.CompletedTask; } },
        new() { Id = "gpu.nv-pstate", Name = "NVIDIA P-State 0", Category = "GPU",
            Description = "Forces GPU to maximum performance state at all times.",
            ShouldApply = hw => hw.IsNvidiaGpu,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "DisableDynamicPstate", out var p) ? p == 1 : null,
            Apply = (p, _) => { p.Report("NVIDIA P-State 0"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "DisableDynamicPstate", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "DisableDynamicPstate"); return Task.CompletedTask; } },
        new() { Id = "gpu.dwm-vblank", Name = "DWM VBlank optimized", Category = "GPU",
            Description = "Disables DWM flip-wait override for reduced compositor latency.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "DisableFlipWaitOverride", out var x) && x == 1,
            Apply = (p, _) => { p.Report("DWM VBlank"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "DisableFlipWaitOverride", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "DisableFlipWaitOverride"); return Task.CompletedTask; } },
        new() { Id = "gpu.game-sched", Name = "GPU game scheduling", Category = "GPU",
            Description = "Sets GPU scheduling priority to 8 for Games tasks.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", out var g) && g == 8,
            Apply = (p, _) => { p.Report("GPU priority 8"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 0); return Task.CompletedTask; } },
        new() { Id = "gpu.nv-telemetry", Name = "NVIDIA telemetry off", Category = "GPU",
            Description = "Disables NVIDIA telemetry service.",
            ShouldApply = hw => hw.IsNvidiaGpu,
            TryGetAppliedState = _ => ServiceProbeHelper.IsServiceDisabled("NvTelemetryContainer"),
            Apply = async (p, ct) => { p.Report("NVIDIA telemetry off"); await ProcessRunner.RunAsync("sc.exe", "config NvTelemetryContainer start= disabled", false, null, ct); await ProcessRunner.RunAsync("sc.exe", "stop NvTelemetryContainer", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("sc.exe", "config NvTelemetryContainer start= auto", false, null, ct); }
        },
        new() { Id = "gpu.hdcp-off", Name = "HDCP off", Category = "GPU",
            Description = "Disables HDCP for reduced processing overhead in games.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "RMHdcpKeyglobZero", out var v) && v == 1,
            Apply = (p, _) => { p.Report("HDCP off"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "RMHdcpKeyglobZero", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "RMHdcpKeyglobZero"); return Task.CompletedTask; }
        },
    ];
}

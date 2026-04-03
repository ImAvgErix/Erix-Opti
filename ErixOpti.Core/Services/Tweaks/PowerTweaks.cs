using System.Text.RegularExpressions;
using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class PowerTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new()
        {
            Id = "pwr.erix-plan", Name = "Erix Gaming power plan", Category = "Power", PlanOrder = -10,
            Description = "Creates a custom max-performance power plan with aggressive boost and no idle.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => PowerCfgHelper.IsActiveSchemeNamedErix(),
            Apply = async (p, ct) =>
            {
                p.Report("Creating Erix Gaming power plan...");
                var (_, stdout, _) = await ProcessRunner.RunAsync("powercfg", $"-duplicatescheme {PowerCfgHelper.UltimatePerformanceTemplate}", false, null, ct);
                var m = Regex.Match(stdout, @"[0-9a-fA-F-]{36}");
                if (!m.Success) { p.Report("Template missing, skipping."); return; }
                var guid = m.Value;
                await ProcessRunner.RunAsync("powercfg", $"-changename {guid} \"Erix Gaming\" \"ErixOpti max-performance plan\"", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", $"-setactive {guid}", false, null, ct);
                var desktop = HwRef.Hw?.IsDesktop != false;
                await ConfigureErixPlan(guid, desktop, p, ct);
            },
            Revert = async (p, ct) => { p.Report("Reverting to Balanced"); await ProcessRunner.RunAsync("powercfg", "-setactive 381b4222-6948-41d0-8b5a-4c4a4b4f4b4d", false, null, ct); }
        },
        new()
        {
            Id = "pwr.usb-suspend", Name = "USB selective suspend off", Category = "Power",
            Description = "Prevents USB devices from sleeping (desktop).",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => { var g = PowerCfgHelper.GetActiveSchemeGuid(); return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupUsb, PowerCfgHelper.UsbSelectiveSuspend) == 0 : null; },
            Apply = async (p, ct) => { p.Report("USB suspend off"); await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupUsb, PowerCfgHelper.UsbSelectiveSuspend, 0, ct); },
            Revert = async (p, ct) => { await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupUsb, PowerCfgHelper.UsbSelectiveSuspend, 1, ct); }
        },
        new()
        {
            Id = "pwr.pcie-off", Name = "PCIe ASPM off", Category = "Power",
            Description = "Disables PCIe Active State Power Management.",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => { var g = PowerCfgHelper.GetActiveSchemeGuid(); return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupPcie, PowerCfgHelper.PcieAspm) == 0 : null; },
            Apply = async (p, ct) => { p.Report("PCIe ASPM off"); await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupPcie, PowerCfgHelper.PcieAspm, 0, ct); },
            Revert = async (p, ct) => { await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupPcie, PowerCfgHelper.PcieAspm, 1, ct); }
        },
        new()
        {
            Id = "pwr.hibernate-off", Name = "Hibernate off", Category = "Power",
            Description = "Disables hibernation, frees disk space equal to RAM.",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => !PowerCfgHelper.IsHibernateEnabled(),
            Apply = async (p, ct) => { p.Report("Hibernate off"); await ProcessRunner.RunAsync("powercfg", "-h off", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-h on", false, null, ct); }
        },
        new()
        {
            Id = "pwr.core-unpark", Name = "Unpark CPU cores", Category = "Power",
            Description = "Sets minimum processor state to 100% (all cores active).",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => { var g = PowerCfgHelper.GetActiveSchemeGuid(); return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMinState) == 100 : null; },
            Apply = async (p, ct) => { p.Report("CPU cores 100%"); await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMinState, 100, ct); },
            Revert = async (p, ct) => { await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMinState, 5, ct); }
        },
        new()
        {
            Id = "pwr.throttle-off", Name = "Power throttling off", Category = "Power",
            Description = "Disables power throttling via registry.",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", out var v) && v == 1,
            Apply = async (p, ct) => { p.Report("Power throttling off"); RegistryTweakHelper.WriteDword(Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1); await Task.CompletedTask; },
            Revert = async (p, ct) => { RegistryTweakHelper.DeleteValue(Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff"); await Task.CompletedTask; }
        },
        new()
        {
            Id = "pwr.hdd-timeout", Name = "Disk never sleep", Category = "Power",
            Description = "Sets hard disk timeout to never.",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => { var g = PowerCfgHelper.GetActiveSchemeGuid(); return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupDisk, PowerCfgHelper.DiskIdle) == 0 : null; },
            Apply = async (p, ct) => { p.Report("Disk → never sleep"); await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupDisk, PowerCfgHelper.DiskIdle, 0, ct); },
            Revert = async (p, ct) => { await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupDisk, PowerCfgHelper.DiskIdle, 3600, ct); }
        },
        new()
        {
            Id = "pwr.cpu-max", Name = "CPU max 100%", Category = "Power",
            Description = "Sets maximum processor state to 100%.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => { var g = PowerCfgHelper.GetActiveSchemeGuid(); return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMaxState) == 100 : null; },
            Apply = async (p, ct) => { p.Report("CPU max 100%"); await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMaxState, 100, ct); },
            Revert = async (p, ct) => { await Pwr("SCHEME_CURRENT", PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMaxState, 100, ct); }
        },
        new()
        {
            Id = "pwr.usb-wmi", Name = "USB WMI power off", Category = "Power",
            Description = "Disables WMI power management for USB devices.",
            ShouldApply = hw => hw.IsDesktop || hw.HasManyUsb,
            TryGetAppliedStateAsync = async (_, ct) =>
            {
                var (code, stdout, _) = await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"(Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | Where-Object { $_.enable -eq $true }).Count\"", false, null, ct);
                return code == 0 && int.TryParse(stdout.Trim(), out var n) ? n == 0 : null;
            },
            Apply = async (p, ct) => { p.Report("USB WMI power off"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $false; $_.psbase.put() }\"", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $true; $_.psbase.put() }\"", false, null, ct); }
        },
        new()
        {
            Id = "pwr.timer-res", Name = "Global timer resolution", Category = "Power",
            Description = "Enables global timer resolution requests for low-latency apps.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "GlobalTimerResolutionRequests", out var v) && v == 1,
            Apply = (p, _) => { p.Report("Global timer resolution on"); RegistryTweakHelper.WriteDword(Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "GlobalTimerResolutionRequests", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(Microsoft.Win32.RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "GlobalTimerResolutionRequests"); return Task.CompletedTask; }
        },
    ];

    private static async Task Pwr(string scheme, string sub, string setting, int val, CancellationToken ct)
    {
        await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex {scheme} {sub} {setting} {val}", false, null, ct);
        await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
    }

    private static async Task ConfigureErixPlan(string guid, bool isDesktop, IProgress<string> p, CancellationToken ct)
    {
        async Task S(string sub, string setting, int ac, int? dc = null)
        {
            await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex {guid} {sub} {setting} {ac}", false, null, ct);
            if (dc.HasValue) await ProcessRunner.RunAsync("powercfg", $"-setdcvalueindex {guid} {sub} {setting} {dc.Value}", false, null, ct);
        }

        p.Report("Configuring Erix plan...");
        await S(PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMinState, 100, isDesktop ? 100 : 5);
        await S(PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMaxState, 100, 100);
        // Boost mode: 2=Aggressive (max turbo always)
        await S(PowerCfgHelper.SubGroupProcessor, "be337238-0d82-4146-a960-4f3749d470c7", 2, isDesktop ? 2 : 1);
        // Processor idle disable: 1=disabled (cores never sleep) -- desktop only
        if (isDesktop)
            await S(PowerCfgHelper.SubGroupProcessor, "5d76a2ca-e8c0-402f-a133-2158492d58ad", 1);
        await S(PowerCfgHelper.SubGroupDisk, PowerCfgHelper.DiskIdle, 0, isDesktop ? 0 : 300);
        await S(PowerCfgHelper.SubGroupUsb, PowerCfgHelper.UsbSelectiveSuspend, 0, isDesktop ? 0 : 1);
        await S(PowerCfgHelper.SubGroupPcie, PowerCfgHelper.PcieAspm, 0, isDesktop ? 0 : 1);
        // Display timeout
        var displaySub = "7516b95f-f776-4464-8c53-06167f40cc99";
        var displayOff = "3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e";
        await S(displaySub, displayOff, isDesktop ? 0 : 600, isDesktop ? 0 : 300);
        await ProcessRunner.RunAsync("powercfg", $"-setactive {guid}", false, null, ct);
    }
}

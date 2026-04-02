using System.Text.RegularExpressions;
using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class PowerTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "pwr.usb-wmi", Name = "USB WMI power off", Category = "Power", ShouldApply = hw => hw.IsDesktop || hw.HasManyUsb,
            Apply = async (p, ct) => { p.Report("USB WMI power disable"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $false; $_.psbase.put() }\"", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $true; $_.psbase.put() }\"", false, null, ct); } },
        new() { Id = "pwr.usb-suspend", Name = "USB selective suspend off", Category = "Power", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("USB selective suspend off"); await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 1", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); } },
        new() { Id = "pwr.pcie-off", Name = "PCIe ASPM off", Category = "Power", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("PCIe ASPM off"); await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PCIEXPRESS ASPM 0", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PCIEXPRESS ASPM 1", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); } },
        new() { Id = "pwr.hibernate-off", Name = "Hibernate off", Category = "Power", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("Hibernate off"); await ProcessRunner.RunAsync("powercfg", "-h off", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-h on", false, null, ct); } },
        new() { Id = "pwr.ultimate", Name = "Ultimate Performance plan", Category = "Power", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("Creating Ultimate Performance plan"); var (_, stdout, _) = await ProcessRunner.RunAsync("powercfg", $"-duplicatescheme {PowerCfgHelper.UltimatePerformanceTemplate}", false, null, ct); var m = Regex.Match(stdout, @"[0-9a-fA-F-]{36}"); if (m.Success) await ProcessRunner.RunAsync("powercfg", $"-setactive {m.Value}", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-setactive 381b4222-6948-41d0-8b5a-4c4a4b4f4b4d", false, null, ct); } },
        new() { Id = "pwr.core-unpark", Name = "Unpark CPU cores", Category = "Power", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("CPU cores 100%"); await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 100", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 5", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); } },
        new() { Id = "pwr.throttle-off", Name = "Power throttling off", Category = "Power", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("Power throttling off"); await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR THROTTLING 0", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR THROTTLING 1", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); } },
        new() { Id = "pwr.hdd-timeout", Name = "Disk never sleep", Category = "Power", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("Disk timeout → never"); await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_DISK DISKIDLE 0", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_DISK DISKIDLE 1200", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); } },
        new() { Id = "pwr.cpu-max", Name = "CPU max performance 100%", Category = "Power", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("CPU max state 100%"); await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} BC5038F7-23E0-4960-96DA-33ABAF5935EC 100", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); },
            Revert = async (p, ct) => { p.Report("Revert CPU max state"); await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} BC5038F7-23E0-4960-96DA-33ABAF5935EC 90", false, null, ct); await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct); } },
    ];
}

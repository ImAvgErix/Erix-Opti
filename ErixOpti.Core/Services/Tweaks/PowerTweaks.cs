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
            ShouldApply = _ => true,
            TryGetAppliedState = _ => PowerCfgHelper.IsActiveSchemeNamedErix(),
            Apply = async (p, ct) =>
            {
                p.Report("Creating Erix Gaming power plan...");
                var (_, stdout, _) = await ProcessRunner.RunAsync("powercfg",
                    $"-duplicatescheme {PowerCfgHelper.UltimatePerformanceTemplate}", false, null, ct);
                var m = Regex.Match(stdout, @"[0-9a-fA-F-]{36}");
                if (!m.Success) { p.Report("Could not create plan — template missing. Falling back to High Performance."); return; }
                var guid = m.Value;
                await ProcessRunner.RunAsync("powercfg", $"-changename {guid} \"Erix Gaming\" \"ErixOpti max-performance plan\"", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", $"-setactive {guid}", false, null, ct);
                var hw = HwRef.Hw;
                await ConfigureErixPlan(guid, hw?.IsDesktop != false, p, ct);
            },
            Revert = async (p, ct) =>
            {
                p.Report("Reverting to Balanced plan");
                await ProcessRunner.RunAsync("powercfg", "-setactive 381b4222-6948-41d0-8b5a-4c4a4b4f4b4d", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.usb-suspend", Name = "USB selective suspend off", Category = "Power",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ =>
            {
                var g = PowerCfgHelper.GetActiveSchemeGuid();
                return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupUsb, PowerCfgHelper.UsbSelectiveSuspend) == 0 : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("USB selective suspend off");
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupUsb} {PowerCfgHelper.UsbSelectiveSuspend} 0", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupUsb} {PowerCfgHelper.UsbSelectiveSuspend} 1", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.pcie-off", Name = "PCIe ASPM off", Category = "Power",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ =>
            {
                var g = PowerCfgHelper.GetActiveSchemeGuid();
                return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupPcie, PowerCfgHelper.PcieAspm) == 0 : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("PCIe ASPM off");
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupPcie} {PowerCfgHelper.PcieAspm} 0", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupPcie} {PowerCfgHelper.PcieAspm} 1", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.hibernate-off", Name = "Hibernate off", Category = "Power",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => !PowerCfgHelper.IsHibernateEnabled(),
            Apply = async (p, ct) =>
            {
                p.Report("Hibernate off");
                await ProcessRunner.RunAsync("powercfg", "-h off", false, null, ct);
            },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powercfg", "-h on", false, null, ct); }
        },
        new()
        {
            Id = "pwr.core-unpark", Name = "Unpark CPU cores", Category = "Power",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ =>
            {
                var g = PowerCfgHelper.GetActiveSchemeGuid();
                return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMinState) == 100 : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("CPU cores 100%");
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 100", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 5", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.throttle-off", Name = "Power throttling off", Category = "Power",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(Microsoft.Win32.RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", out var v) && v == 1,
            Apply = async (p, ct) =>
            {
                p.Report("Power throttling off");
                RegistryTweakHelper.WriteDword(Microsoft.Win32.RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                RegistryTweakHelper.DeleteValue(Microsoft.Win32.RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff");
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.hdd-timeout", Name = "Disk never sleep", Category = "Power",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ =>
            {
                var g = PowerCfgHelper.GetActiveSchemeGuid();
                return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupDisk, PowerCfgHelper.DiskIdle) == 0 : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("Disk timeout → never");
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupDisk} {PowerCfgHelper.DiskIdle} 0", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupDisk} {PowerCfgHelper.DiskIdle} 3600", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.cpu-max", Name = "CPU max performance 100%", Category = "Power",
            ShouldApply = _ => true,
            TryGetAppliedState = _ =>
            {
                var g = PowerCfgHelper.GetActiveSchemeGuid();
                return g is not null ? PowerCfgHelper.ReadAcSettingIndex(g, PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMaxState) == 100 : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("CPU max state 100%");
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMaxState} 100", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMaxState} 100", false, null, ct);
                await ProcessRunner.RunAsync("powercfg", "-setactive SCHEME_CURRENT", false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.usb-wmi", Name = "USB WMI power off", Category = "Power",
            ShouldApply = hw => hw.IsDesktop || hw.HasManyUsb,
            TryGetAppliedStateAsync = async (_, ct) =>
            {
                var (code, stdout, _) = await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"(Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | Where-Object { $_.enable -eq $true }).Count\"",
                    false, null, ct);
                return code == 0 && int.TryParse(stdout.Trim(), out var n) ? n == 0 : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("USB WMI power disable");
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $false; $_.psbase.put() }\"",
                    false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $true; $_.psbase.put() }\"",
                    false, null, ct);
            }
        },
        new()
        {
            Id = "pwr.timer-res", Name = "Global timer resolution requests", Category = "Power",
            ShouldApply = _ => true,
            TryGetAppliedState = _ =>
                RegistryTweakHelper.TryReadDword(Microsoft.Win32.RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "GlobalTimerResolutionRequests", out var v) && v == 1,
            Apply = (p, _) =>
            {
                p.Report("Enable global timer resolution requests");
                RegistryTweakHelper.WriteDword(Microsoft.Win32.RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "GlobalTimerResolutionRequests", 1);
                return Task.CompletedTask;
            },
            Revert = (p, _) =>
            {
                RegistryTweakHelper.DeleteValue(Microsoft.Win32.RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "GlobalTimerResolutionRequests");
                return Task.CompletedTask;
            }
        },
    ];

    private static async Task ConfigureErixPlan(string guid, bool isDesktop, IProgress<string> p, CancellationToken ct)
    {
        async Task Set(string sub, string setting, int ac, int? dc = null)
        {
            await ProcessRunner.RunAsync("powercfg", $"-setacvalueindex {guid} {sub} {setting} {ac}", false, null, ct);
            if (dc.HasValue)
                await ProcessRunner.RunAsync("powercfg", $"-setdcvalueindex {guid} {sub} {setting} {dc.Value}", false, null, ct);
        }

        p.Report("Configuring Erix plan settings...");
        await Set(PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMinState, 100, isDesktop ? 100 : 5);
        await Set(PowerCfgHelper.SubGroupProcessor, PowerCfgHelper.ProcessorMaxState, 100, 100);
        await Set(PowerCfgHelper.SubGroupDisk, PowerCfgHelper.DiskIdle, 0, isDesktop ? 0 : 300);
        await Set(PowerCfgHelper.SubGroupUsb, PowerCfgHelper.UsbSelectiveSuspend, 0, isDesktop ? 0 : 1);
        await Set(PowerCfgHelper.SubGroupPcie, PowerCfgHelper.PcieAspm, 0, isDesktop ? 0 : 1);
        // Display timeout (seconds): desktop = never, laptop = 300
        await Set(PowerCfgHelper.SubGroupSleep, "29f6c1db-86da-48c5-9fdb-f2b67b1f44da", isDesktop ? 0 : 600, isDesktop ? 0 : 300);
        // Sleep timeout
        await Set(PowerCfgHelper.SubGroupSleep, "29f6c1db-86da-48c5-9fdb-f2b67b1f44da", isDesktop ? 0 : 1800, isDesktop ? 0 : 900);
        // Turn off display
        var displaySub = "7516b95f-f776-4464-8c53-06167f40cc99";
        var displayOff = "3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e";
        await Set(displaySub, displayOff, isDesktop ? 0 : 600, isDesktop ? 0 : 300);
        await ProcessRunner.RunAsync("powercfg", $"-setactive {guid}", false, null, ct);
    }
}

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class NetworkTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new()
        {
            Id = "net.tcp", Name = "TCP optimization", Category = "Network",
            Description = "Disables heuristics, timestamps, ECN; enables RSS.",
            ShouldApply = _ => true,
            TryGetAppliedStateAsync = async (_, ct) =>
            {
                var (code, stdout, _) = await ProcessRunner.RunAsync("netsh", "int tcp show global", false, null, ct);
                if (code != 0) return null;
                return stdout.Contains("disabled", StringComparison.OrdinalIgnoreCase) ? true : false;
            },
            Apply = async (p, ct) =>
            {
                p.Report("TCP stack optimize");
                await ProcessRunner.RunAsync("netsh", "int tcp set heuristics disabled", false, null, ct);
                await ProcessRunner.RunAsync("netsh", "int tcp set global autotuninglevel=normal", false, null, ct);
                await ProcessRunner.RunAsync("netsh", "int tcp set global rss=enabled", false, null, ct);
                await ProcessRunner.RunAsync("netsh", "int tcp set global timestamps=disabled", false, null, ct);
                await ProcessRunner.RunAsync("netsh", "int tcp set global ecncapability=disabled", false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("netsh", "int tcp set heuristics enabled", false, null, ct);
                await ProcessRunner.RunAsync("netsh", "int tcp set global timestamps=enabled", false, null, ct);
                await ProcessRunner.RunAsync("netsh", "int tcp set global ecncapability=default", false, null, ct);
            }
        },
        new()
        {
            Id = "net.nagle", Name = "Nagle off", Category = "Network",
            Description = "Disables Nagle's algorithm for lower latency.",
            ShouldApply = _ => true,
            TryGetAppliedStateAsync = async (_, ct) =>
            {
                var (code, stdout, _) = await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"$r=Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { (Get-ItemProperty $_.PSPath -Name TCPNoDelay -EA SilentlyContinue).TCPNoDelay }; if ($r -contains 1) { 'on' } else { 'off' }\"",
                    false, null, ct);
                return code == 0 ? stdout.Trim() == "on" : null;
            },
            Apply = async (p, ct) =>
            {
                p.Report("Nagle off");
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { Set-ItemProperty $_.PSPath -Name TcpAckFrequency -Value 1 -EA SilentlyContinue; Set-ItemProperty $_.PSPath -Name TCPNoDelay -Value 1 -EA SilentlyContinue }\"",
                    false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { Remove-ItemProperty $_.PSPath -Name TcpAckFrequency -EA SilentlyContinue; Remove-ItemProperty $_.PSPath -Name TCPNoDelay -EA SilentlyContinue }\"",
                    false, null, ct);
            }
        },
        new()
        {
            Id = "net.lso-off", Name = "Large Send Offload off", Category = "Network",
            Description = "Disables LSO for more consistent packet handling.",
            ShouldApply = _ => true,
            TryGetAppliedStateAsync = async (_, ct) =>
            {
                var (code, stdout, _) = await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"$a=Get-NetAdapterAdvancedProperty -DisplayName '*Large Send Offload*' -EA SilentlyContinue; if ($a) { ($a | Where-Object RegistryValue -ne 0).Count -eq 0 } else { 'na' }\"",
                    false, null, ct);
                if (code != 0) return null;
                var t = stdout.Trim();
                return t == "na" ? true : t == "True";
            },
            Apply = async (p, ct) =>
            {
                p.Report("LSO off");
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-NetAdapterAdvancedProperty -DisplayName '*Large Send Offload*' -EA SilentlyContinue | Set-NetAdapterAdvancedProperty -RegistryValue 0 -EA SilentlyContinue\"",
                    false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-NetAdapterAdvancedProperty -DisplayName '*Large Send Offload*' -EA SilentlyContinue | Set-NetAdapterAdvancedProperty -RegistryValue 1 -EA SilentlyContinue\"",
                    false, null, ct);
            }
        },
        new()
        {
            Id = "net.nic-power", Name = "NIC power management off", Category = "Network",
            Description = "Prevents NIC from sleeping (desktop only).",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                p.Report("NIC power saving off");
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-NetAdapter -Physical | ForEach-Object { $path = 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Class\\' + $_.InterfaceGuid; Set-ItemProperty -Path $path -Name 'PnPCapabilities' -Value 24 -Type DWord -EA SilentlyContinue }\"",
                    false, null, ct);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"Get-NetAdapter -Physical | ForEach-Object { $path = 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Class\\' + $_.InterfaceGuid; Remove-ItemProperty -Path $path -Name 'PnPCapabilities' -EA SilentlyContinue }\"",
                    false, null, ct);
            }
        },
        new()
        {
            Id = "net.dns-cache", Name = "DNS cache optimized", Category = "Network",
            Description = "Sets DNS cache TTL to 24h, negative TTL to 5s.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ =>
            {
                var a = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxCacheTtl", out var ttl) && ttl == 86400;
                var b = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxNegativeCacheTtl", out var neg) && neg == 5;
                return a && b;
            },
            Apply = (p, _) =>
            {
                p.Report("DNS cache optimize");
                RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxCacheTtl", 86400);
                RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxNegativeCacheTtl", 5);
                return Task.CompletedTask;
            },
            Revert = (p, _) =>
            {
                RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxCacheTtl");
                RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxNegativeCacheTtl");
                return Task.CompletedTask;
            }
        },
        new()
        {
            Id = "net.smb-throttle", Name = "SMB throttling off", Category = "Network",
            Description = "Disables SMB bandwidth throttling for faster file transfers.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters", "DisableBandwidthThrottling", out var v) && v == 1,
            Apply = (p, _) => { p.Report("SMB throttling off"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters", "DisableBandwidthThrottling", 1); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters", "DisableBandwidthThrottling"); return Task.CompletedTask; }
        },
        new()
        {
            Id = "net.llmnr-off", Name = "LLMNR off", Category = "Network",
            Description = "Disables Link-Local Multicast Name Resolution.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\DNSClient", "EnableMulticast", out var v) && v == 0,
            Apply = (p, _) => { p.Report("LLMNR off"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\DNSClient", "EnableMulticast", 0); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows NT\DNSClient", "EnableMulticast"); return Task.CompletedTask; }
        },
    ];
}

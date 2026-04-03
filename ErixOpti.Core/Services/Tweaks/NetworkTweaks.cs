using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class NetworkTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "net.tcp", Name = "TCP optimization", Category = "Network", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("TCP stack optimize"); await ProcessRunner.RunAsync("netsh", "int tcp set heuristics disabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global autotuninglevel=normal", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global rss=enabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global timestamps=disabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global ecncapability=disabled", false, null, ct); },
            Revert = async (p, ct) => { p.Report("Revert TCP stack"); await ProcessRunner.RunAsync("netsh", "int tcp set heuristics enabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global autotuninglevel=normal", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global rss=enabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global timestamps=enabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global ecncapability=default", false, null, ct); } },
        new() { Id = "net.nagle", Name = "Nagle off", Category = "Network", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("Nagle off"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { Set-ItemProperty $_.PSPath -Name TcpAckFrequency -Value 1 -EA SilentlyContinue; Set-ItemProperty $_.PSPath -Name TCPNoDelay -Value 1 -EA SilentlyContinue }\"", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { Remove-ItemProperty $_.PSPath -Name TcpAckFrequency -EA SilentlyContinue; Remove-ItemProperty $_.PSPath -Name TCPNoDelay -EA SilentlyContinue }\"", false, null, ct); } },
        new() { Id = "net.lso-off", Name = "Large Send Offload off", Category = "Network", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("LSO off"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-NetAdapterAdvancedProperty -DisplayName '*Large Send Offload*' -EA SilentlyContinue | Set-NetAdapterAdvancedProperty -RegistryValue 0 -EA SilentlyContinue\"", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-NetAdapterAdvancedProperty -DisplayName '*Large Send Offload*' -EA SilentlyContinue | Set-NetAdapterAdvancedProperty -RegistryValue 1 -EA SilentlyContinue\"", false, null, ct); } },
        new() { Id = "net.nic-power", Name = "NIC power management off", Category = "Network", ShouldApply = hw => hw.IsDesktop,
            Apply = async (p, ct) => { p.Report("NIC power saving off"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-NetAdapter -Physical | ForEach-Object { $path = 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Class\\' + $_.InterfaceGuid; Set-ItemProperty -Path $path -Name 'PnPCapabilities' -Value 24 -Type DWord -EA SilentlyContinue }\"", false, null, ct); },
            Revert = async (p, ct) => { p.Report("Revert NIC power"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-NetAdapter -Physical | ForEach-Object { $path = 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Class\\' + $_.InterfaceGuid; Remove-ItemProperty -Path $path -Name 'PnPCapabilities' -EA SilentlyContinue }\"", false, null, ct); } },
        new() { Id = "net.dns-cache", Name = "DNS cache optimized", Category = "Network", ShouldApply = _ => true,
            TryGetAppliedState = _ =>
            {
                var a = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxCacheTtl", out var ttl) && ttl == 86400;
                var b = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters", "MaxNegativeCacheTtl", out var neg) && neg == 5;
                return a && b;
            },
            Apply = async (p, ct) => { p.Report("Optimize DNS cache"); await ProcessRunner.RunAsync("reg", "add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\Parameters\" /v MaxCacheTtl /t REG_DWORD /d 86400 /f", false, null, ct); await ProcessRunner.RunAsync("reg", "add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\Parameters\" /v MaxNegativeCacheTtl /t REG_DWORD /d 5 /f", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("reg", "delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\Parameters\" /v MaxCacheTtl /f", false, null, ct); await ProcessRunner.RunAsync("reg", "delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\Parameters\" /v MaxNegativeCacheTtl /f", false, null, ct); } },
    ];
}

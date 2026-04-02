using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class NetworkTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "net.tcp", Name = "TCP optimization", Category = "Network", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("TCP stack optimize"); await ProcessRunner.RunAsync("netsh", "int tcp set heuristics disabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global autotuninglevel=normal", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global rss=enabled", false, null, ct); await ProcessRunner.RunAsync("netsh", "int tcp set global timestamps=disabled", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("netsh", "int tcp set heuristics enabled", false, null, ct); } },
        new() { Id = "net.nagle", Name = "Nagle off", Category = "Network", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("Nagle off"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { Set-ItemProperty $_.PSPath -Name TcpAckFrequency -Value 1 -EA SilentlyContinue; Set-ItemProperty $_.PSPath -Name TCPNoDelay -Value 1 -EA SilentlyContinue }\"", false, null, ct); },
            Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces' | ForEach-Object { Remove-ItemProperty $_.PSPath -Name TcpAckFrequency -EA SilentlyContinue; Remove-ItemProperty $_.PSPath -Name TCPNoDelay -EA SilentlyContinue }\"", false, null, ct); } },
    ];
}

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class CleanupTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "clean.temp", Name = "Clean TEMP", Category = "Cleanup", ShouldApply = _ => true,
            Apply = (p, _) => { p.Report("Cleaning TEMP"); CleanDir(Path.GetTempPath()); CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")); return Task.CompletedTask; },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.prefetch", Name = "Clean Prefetch", Category = "Cleanup", ShouldApply = _ => true,
            Apply = (p, _) => { p.Report("Cleaning Prefetch"); CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch")); return Task.CompletedTask; },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.softdist", Name = "Clean WU cache", Category = "Cleanup", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("Cleaning WU cache"); await ProcessRunner.RunAsync("net", "stop wuauserv", false, null, ct); CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download")); await ProcessRunner.RunAsync("net", "start wuauserv", false, null, ct); },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.events", Name = "Clear event logs", Category = "Cleanup", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("Clearing event logs"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Get-WinEvent -ListLog * -EA SilentlyContinue | ForEach-Object { try{[System.Diagnostics.Eventing.Reader.EventLogSession]::GlobalSession.ClearLog($_.LogName)}catch{} }\"", false, null, ct); },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.dism", Name = "DISM cleanup", Category = "Cleanup", ShouldApply = _ => true,
            Apply = async (p, ct) => { p.Report("DISM component cleanup"); await ProcessRunner.RunAsync("dism.exe", "/Online /Cleanup-Image /StartComponentCleanup", false, null, ct); },
            Revert = (_, _) => Task.CompletedTask },
    ];

    private static void CleanDir(string path)
    {
        if (!Directory.Exists(path)) return;
        try { foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)) try { File.Delete(f); } catch { } } catch { }
        try { foreach (var d in Directory.EnumerateDirectories(path)) try { Directory.Delete(d, true); } catch { } } catch { }
    }
}

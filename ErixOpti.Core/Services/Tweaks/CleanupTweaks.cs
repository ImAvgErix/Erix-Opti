using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class CleanupTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "clean.temp", Name = "Clean TEMP", Category = "Cleanup",
            Description = "Deletes temporary files from user and system TEMP folders.",
            ShouldApply = _ => true,
            Apply = (p, _) => { var (f1, b1) = CleanDir(Path.GetTempPath()); var (f2, b2) = CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")); p.Report($"TEMP: {f1 + f2} files, {Fmt(b1 + b2)} freed"); return Task.CompletedTask; },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.prefetch", Name = "Clean Prefetch", Category = "Cleanup",
            Description = "Clears Windows Prefetch cache.",
            ShouldApply = _ => true,
            Apply = (p, _) => { var (f, b) = CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch")); p.Report($"Prefetch: {f} files, {Fmt(b)} freed"); return Task.CompletedTask; },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.softdist", Name = "Clean WU cache", Category = "Cleanup",
            Description = "Stops Windows Update and clears the download cache.",
            ShouldApply = _ => true,
            Apply = async (p, ct) =>
            {
                p.Report("Stopping wuauserv...");
                await ProcessRunner.RunAsync("net", "stop wuauserv", false, null, ct);
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                var (f, b) = CleanDir(path);
                p.Report($"WU cache: {f} files, {Fmt(b)} freed");
                await ProcessRunner.RunAsync("net", "start wuauserv", false, null, ct);
            },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.events", Name = "Clear event logs", Category = "Cleanup",
            Description = "Clears all Windows event logs.",
            ShouldApply = _ => true,
            Apply = async (p, ct) =>
            {
                p.Report("Clearing event logs...");
                var (code, stdout, _) = await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"$c=0; Get-WinEvent -ListLog * -EA SilentlyContinue | ForEach-Object { try{[System.Diagnostics.Eventing.Reader.EventLogSession]::GlobalSession.ClearLog($_.LogName);$c++}catch{} }; Write-Output $c\"",
                    false, null, ct);
                p.Report(code == 0 ? $"Cleared {stdout.Trim()} event logs" : $"Partial clear (exit {code})");
            },
            Revert = (_, _) => Task.CompletedTask },
        new() { Id = "clean.dism", Name = "DISM cleanup", Category = "Cleanup",
            Description = "Runs DISM component cleanup to reclaim disk space.",
            ShouldApply = _ => true,
            Apply = async (p, ct) =>
            {
                p.Report("DISM cleanup (may take a minute)...");
                var (code, _, stderr) = await ProcessRunner.RunAsync("dism.exe", "/Online /Cleanup-Image /StartComponentCleanup", false, null, ct);
                p.Report(code == 0 ? "DISM cleanup complete." : $"DISM code {code}: {stderr.Trim()}");
            },
            Revert = (_, _) => Task.CompletedTask },
    ];

    private static (int files, long bytes) CleanDir(string path)
    {
        int files = 0; long bytes = 0;
        if (!Directory.Exists(path)) return (0, 0);
        try { foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)) { try { var i = new FileInfo(f); var sz = i.Length; i.Delete(); files++; bytes += sz; } catch { } } } catch { }
        try { foreach (var d in Directory.EnumerateDirectories(path)) { try { Directory.Delete(d, true); } catch { } } } catch { }
        return (files, bytes);
    }

    private static string Fmt(long b) => b switch { >= 1_073_741_824 => $"{b / 1073741824.0:0.#} GB", >= 1_048_576 => $"{b / 1048576.0:0.#} MB", >= 1024 => $"{b / 1024.0:0.#} KB", _ => $"{b} B" };
}

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class CleanupTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        new() { Id = "clean.temp", Name = "Clean TEMP", Category = "Cleanup", ShouldApply = _ => true,
            Apply = (p, _) =>
            {
                var (files, bytes) = CleanDir(Path.GetTempPath());
                var (f2, b2) = CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));
                p.Report($"TEMP: deleted {files + f2} files, freed {FormatBytes(bytes + b2)}");
                return Task.CompletedTask;
            },
            Revert = (_, _) => Task.CompletedTask },

        new() { Id = "clean.prefetch", Name = "Clean Prefetch", Category = "Cleanup", ShouldApply = _ => true,
            Apply = (p, _) =>
            {
                var (files, bytes) = CleanDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"));
                p.Report($"Prefetch: deleted {files} files, freed {FormatBytes(bytes)}");
                return Task.CompletedTask;
            },
            Revert = (_, _) => Task.CompletedTask },

        new() { Id = "clean.softdist", Name = "Clean WU cache", Category = "Cleanup", ShouldApply = _ => true,
            Apply = async (p, ct) =>
            {
                p.Report("Stopping Windows Update...");
                var (stopCode, _, stopErr) = await ProcessRunner.RunAsync("net", "stop wuauserv", false, null, ct);
                if (stopCode != 0) p.Report($"Warning: could not stop wuauserv ({stopErr.Trim()})");

                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                var (files, bytes) = CleanDir(path);
                p.Report($"WU cache: deleted {files} files, freed {FormatBytes(bytes)}");

                await ProcessRunner.RunAsync("net", "start wuauserv", false, null, ct);
            },
            Revert = (_, _) => Task.CompletedTask },

        new() { Id = "clean.events", Name = "Clear event logs", Category = "Cleanup", ShouldApply = _ => true,
            Apply = async (p, ct) =>
            {
                p.Report("Clearing event logs...");
                var (code, stdout, stderr) = await ProcessRunner.RunAsync("powershell.exe",
                    "-NoProfile -Command \"$c=0; Get-WinEvent -ListLog * -EA SilentlyContinue | ForEach-Object { try{[System.Diagnostics.Eventing.Reader.EventLogSession]::GlobalSession.ClearLog($_.LogName);$c++}catch{} }; Write-Output $c\"",
                    false, null, ct);
                var cleared = stdout.Trim();
                p.Report(code == 0 ? $"Event logs: cleared {cleared} logs" : $"Event logs: partial (exit {code})");
            },
            Revert = (_, _) => Task.CompletedTask },

        new() { Id = "clean.dism", Name = "DISM cleanup", Category = "Cleanup", ShouldApply = _ => true,
            Apply = async (p, ct) =>
            {
                p.Report("DISM component cleanup (may take a minute)...");
                var (code, _, stderr) = await ProcessRunner.RunAsync("dism.exe",
                    "/Online /Cleanup-Image /StartComponentCleanup", false, null, ct);
                p.Report(code == 0 ? "DISM cleanup complete." : $"DISM finished with code {code}: {stderr.Trim()}");
            },
            Revert = (_, _) => Task.CompletedTask },
    ];

    private static (int files, long bytes) CleanDir(string path)
    {
        int files = 0; long bytes = 0;
        if (!Directory.Exists(path)) return (0, 0);

        try
        {
            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var info = new FileInfo(f);
                    var size = info.Length;
                    info.Delete();
                    files++;
                    bytes += size;
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }
        catch (UnauthorizedAccessException) { }

        try
        {
            foreach (var d in Directory.EnumerateDirectories(path))
            {
                try { Directory.Delete(d, true); }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }
        catch (UnauthorizedAccessException) { }

        return (files, bytes);
    }

    private static string FormatBytes(long bytes) => bytes switch
    {
        >= 1_073_741_824 => $"{bytes / 1073741824.0:0.#} GB",
        >= 1_048_576 => $"{bytes / 1048576.0:0.#} MB",
        >= 1024 => $"{bytes / 1024.0:0.#} KB",
        _ => $"{bytes} B"
    };
}

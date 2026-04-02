using System.Diagnostics;
using System.Management;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Helpers;

namespace ErixOpti.Core.Services;

public sealed class BackupService : IBackupService
{
    public string BackupRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ErixOpti",
        "Backups");

    public async Task<BackupResult> CreateFullBackupAsync(IProgress<string> progress, CancellationToken ct)
    {
        if (!AdminHelper.IsRunningAsAdministrator())
        {
            return new BackupResult(false, null, null, null, "Administrator rights are required to create backups.");
        }

        Directory.CreateDirectory(BackupRoot);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var folder = Path.Combine(BackupRoot, stamp);
        Directory.CreateDirectory(folder);

        string? restoreId = null;
        try
        {
            progress.Report("Creating System Restore point…");
            restoreId = await CreateRestorePointAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            progress.Report($"Restore point warning: {ex.Message}");
        }

        string? regPath = null;
        try
        {
            progress.Report("Exporting registry hives…");
            regPath = Path.Combine(folder, "registry");
            Directory.CreateDirectory(regPath);
            await ExportRegistryAsync(regPath, progress, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new BackupResult(false, restoreId, null, null, $"Registry export failed: {ex.Message}");
        }

        string? bcdPath = null;
        try
        {
            progress.Report("Exporting BCD…");
            bcdPath = Path.Combine(folder, "bcd-backup.bcd");
            var (code, _, err) = await ProcessRunner.RunAsync(
                "bcdedit",
                $"/export \"{bcdPath}\"",
                runElevated: false,
                progress,
                ct).ConfigureAwait(false);
            if (code != 0)
            {
                throw new InvalidOperationException(err);
            }
        }
        catch (Exception ex)
        {
            return new BackupResult(false, restoreId, regPath, null, $"BCD export failed: {ex.Message}");
        }

        return new BackupResult(true, restoreId, regPath, bcdPath, null);
    }

    private static async Task<string?> CreateRestorePointAsync(CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var mc = new ManagementClass("root/default:SystemRestore");
            using var outParams = (ManagementBaseObject)mc.InvokeMethod(
                "CreateRestorePoint",
                new object[] { "ErixOpti backup", 0, 100 });

            return outParams?["ReturnValue"]?.ToString() ?? "created";
        }, ct).ConfigureAwait(false);
    }

    private static async Task ExportRegistryAsync(string folder, IProgress<string> progress, CancellationToken ct)
    {
        var exports = new (string Hive, string FileName)[]
        {
            ("HKLM", "hklm.reg"),
            ("HKCU", "hkcu.reg"),
            ("HKCR", "hkcr.reg"),
            ("HKU", "hku.reg"),
            ("HKCC", "hkcc.reg")
        };

        foreach (var (hive, file) in exports)
        {
            ct.ThrowIfCancellationRequested();
            var path = Path.Combine(folder, file);
            progress.Report($"Exporting {file}…");
            var psi = new ProcessStartInfo("reg.exe", $"export {hive} \"{path}\" /y")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc is null)
            {
                throw new InvalidOperationException("Failed to start reg.exe");
            }

            await proc.WaitForExitAsync(ct).ConfigureAwait(false);
            if (proc.ExitCode != 0)
            {
                var err = await proc.StandardError.ReadToEndAsync(ct).ConfigureAwait(false);
                throw new InvalidOperationException($"reg export failed ({file}): {err}");
            }
        }
    }
}

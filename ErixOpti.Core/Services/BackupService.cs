using System.Diagnostics;
using System.Management;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Helpers;

namespace ErixOpti.Core.Services;

public sealed class BackupService : IBackupService
{
    private static readonly string[] TargetedRegistryPaths =
    [
        @"HKLM\SYSTEM\CurrentControlSet\Services\mouclass\Parameters",
        @"HKLM\SYSTEM\CurrentControlSet\Services\kbdclass\Parameters",
        @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
        @"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl",
        @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
        @"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
        @"HKLM\SOFTWARE\Microsoft\Windows\Dwm",
        @"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
        @"HKLM\SOFTWARE\Policies\Microsoft\Windows\System",
        @"HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search",
        @"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR",
        @"HKLM\SYSTEM\CurrentControlSet\Control",
        @"HKCU\Control Panel\Desktop",
        @"HKCU\Software\Microsoft\GameBar",
        @"HKCU\System\GameConfigStore",
        @"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
        @"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
        @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
        @"HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
        @"HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
        @"HKCU\Software\Policies\Microsoft\Windows\Explorer",
        @"HKCU\Software\NVIDIA Corporation\Global\NVTweak",
    ];

    public string BackupRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ErixOpti", "Backups");

    public async Task<BackupResult> CreateFullBackupAsync(IProgress<string> progress, CancellationToken ct)
    {
        if (!AdminHelper.IsRunningAsAdministrator())
            return new BackupResult(false, null, null, null, "Administrator rights required.");

        Directory.CreateDirectory(BackupRoot);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var folder = Path.Combine(BackupRoot, stamp);
        Directory.CreateDirectory(folder);

        string? restoreId = null;
        try
        {
            progress.Report("Creating System Restore point...");
            restoreId = await CreateRestorePointAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            progress.Report($"Restore point skipped: {ex.Message}");
        }

        string? regPath = null;
        try
        {
            progress.Report("Backing up registry keys...");
            regPath = Path.Combine(folder, "registry");
            Directory.CreateDirectory(regPath);
            await ExportTargetedRegistryAsync(regPath, progress, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            progress.Report($"Registry backup warning: {ex.Message}");
            // Non-fatal: continue with optimization even if some keys couldn't be exported
        }

        string? bcdPath = null;
        try
        {
            progress.Report("Backing up BCD...");
            bcdPath = Path.Combine(folder, "bcd-backup.bcd");
            var (code, _, err) = await ProcessRunner.RunAsync(
                "bcdedit", $"/export \"{bcdPath}\"", false, null, ct).ConfigureAwait(false);
            if (code != 0)
                progress.Report($"BCD backup warning: {err}");
        }
        catch (Exception ex)
        {
            progress.Report($"BCD backup skipped: {ex.Message}");
        }

        progress.Report("Backup complete.");
        return new BackupResult(true, restoreId, regPath, bcdPath, null);
    }

    private static async Task<string?> CreateRestorePointAsync(CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var mc = new ManagementClass("root/default:SystemRestore");
                using var inParams = mc.GetMethodParameters("CreateRestorePoint");
                inParams["Description"] = "ErixOpti backup";
                inParams["RestorePointType"] = 0;
                inParams["EventType"] = 100;
                using var outParams = mc.InvokeMethod("CreateRestorePoint", inParams, null);
                return outParams?["ReturnValue"]?.ToString() ?? "created";
            }
            catch
            {
                return "skipped";
            }
        }, ct).ConfigureAwait(false);
    }

    private static async Task ExportTargetedRegistryAsync(string folder, IProgress<string> progress, CancellationToken ct)
    {
        int exported = 0, skipped = 0;
        foreach (var keyPath in TargetedRegistryPaths)
        {
            ct.ThrowIfCancellationRequested();
            var safeName = keyPath.Replace('\\', '_').Replace(' ', '-') + ".reg";
            var dest = Path.Combine(folder, safeName);

            var psi = new ProcessStartInfo("reg.exe", $"export \"{keyPath}\" \"{dest}\" /y")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using var proc = Process.Start(psi);
                if (proc is null) { skipped++; continue; }
                await proc.WaitForExitAsync(ct).ConfigureAwait(false);
                if (proc.ExitCode == 0) exported++;
                else skipped++;
            }
            catch
            {
                skipped++;
            }
        }
        progress.Report($"Registry: {exported} keys backed up, {skipped} skipped.");
    }
}

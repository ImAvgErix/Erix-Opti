using System.Text;

namespace ErixOpti.Core.Helpers;

/// <summary>
/// Runs commands with TrustedInstaller when a launcher is available: MinSudo, NirSoft AdvancedRun (/RunAs 8), ExecTI/RunAsTI, or plain elevated cmd.
/// </summary>
public static class TrustedInstallerHelper
{
    public static string? ResolveMinSudoPath()
    {
        var p = Path.Combine(AppContext.BaseDirectory, "Assets", "MinSudo.exe");
        if (File.Exists(p))
        {
            return p;
        }

        p = Path.Combine(AppContext.BaseDirectory, "MinSudo.exe");
        if (File.Exists(p))
        {
            return p;
        }

        return BundledToolResolver.FindFile("MinSudo.exe");
    }

    public static string? ResolveRunAsTiCmdPath()
    {
        var dir = AppContext.BaseDirectory;
        foreach (var rel in new[] { Path.Combine("Assets", "RunAsTI.cmd"), "RunAsTI.cmd" })
        {
            var p = Path.Combine(dir, rel);
            if (File.Exists(p))
            {
                return p;
            }
        }

        return null;
    }

    public static async Task<(int ExitCode, string StdOut, string StdErr)> RunProtectedCommandAsync(
        string commandLine,
        IProgress<string>? progress,
        CancellationToken ct)
    {
        progress?.Report($"TI-wrap: {commandLine}");

        var minSudo = ResolveMinSudoPath();
        if (minSudo is not null)
        {
            var args = $"-TrustedInstaller -Wait cmd.exe /c {commandLine}";
            return await ProcessRunner.RunAsync(minSudo, args, false, progress, ct).ConfigureAwait(false);
        }

        var adv = BundledToolResolver.ResolveAdvancedRun();
        if (adv is not null)
        {
            return await RunViaAdvancedRunTrustedInstallerAsync(adv, commandLine, progress, ct).ConfigureAwait(false);
        }

        var execTi = BundledToolResolver.ResolveExecTiLauncher();
        if (execTi is not null)
        {
            if (execTi.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return await ProcessRunner.RunAsync(execTi, commandLine, false, progress, ct).ConfigureAwait(false);
            }

            if (execTi.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                return await ProcessRunner.RunAsync("cmd.exe", $"/c \"\"{execTi}\"\" {commandLine}", false, progress, ct)
                    .ConfigureAwait(false);
            }
        }

        var runAsTi = ResolveRunAsTiCmdPath();
        if (runAsTi is not null)
        {
            return await ProcessRunner.RunAsync("cmd.exe", $"/c \"\"{runAsTi}\"\" {commandLine}", false, progress, ct)
                .ConfigureAwait(false);
        }

        return await ProcessRunner.RunAsync("cmd.exe", $"/c {commandLine}", false, progress, ct).ConfigureAwait(false);
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunViaAdvancedRunTrustedInstallerAsync(
        string advancedRunPath,
        string commandLine,
        IProgress<string>? progress,
        CancellationToken ct)
    {
        var bat = Path.Combine(Path.GetTempPath(), $"erix-ti-{Guid.NewGuid():N}.cmd");
        await File.WriteAllTextAsync(bat, "@echo off\r\n" + commandLine + "\r\n", Encoding.UTF8, ct).ConfigureAwait(false);
        try
        {
            var comspec = Environment.GetEnvironmentVariable("COMSPEC") ?? @"C:\Windows\System32\cmd.exe";
            var args = $"/EXEFilename \"{comspec}\" /CommandLine \"/c \"\"{bat}\"\"\" /RunAs 8 /Run";
            return await ProcessRunner.RunAsync(advancedRunPath, args, false, progress, ct).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                File.Delete(bat);
            }
            catch
            {
                // ignore
            }
        }
    }

    public static bool HasTrustedInstallerLauncher() =>
        ResolveMinSudoPath() is not null ||
        BundledToolResolver.ResolveAdvancedRun() is not null ||
        BundledToolResolver.ResolveExecTiLauncher() is not null ||
        ResolveRunAsTiCmdPath() is not null;
}

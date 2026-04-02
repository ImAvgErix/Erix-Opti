using System.Diagnostics;

namespace ErixOpti.Core.Helpers;

public static class ProcessRunner
{
    public static async Task<(int ExitCode, string StdOut, string StdErr)> RunAsync(
        string fileName,
        string arguments,
        bool runElevated,
        IProgress<string>? progress,
        CancellationToken ct)
    {
        progress?.Report($"{fileName} {arguments}");

        var psi = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (runElevated)
        {
            psi.UseShellExecute = true;
            psi.RedirectStandardOutput = false;
            psi.RedirectStandardError = false;
            psi.Verb = "runas";
            psi.CreateNoWindow = false;
        }

        using var proc = new Process { StartInfo = psi };
        if (!proc.Start())
        {
            return (-1, string.Empty, "Failed to start process.");
        }

        if (!runElevated)
        {
            var stdout = await proc.StandardOutput.ReadToEndAsync(ct).ConfigureAwait(false);
            var stderr = await proc.StandardError.ReadToEndAsync(ct).ConfigureAwait(false);
            await proc.WaitForExitAsync(ct).ConfigureAwait(false);
            return (proc.ExitCode, stdout, stderr);
        }

        await proc.WaitForExitAsync(ct).ConfigureAwait(false);
        return (proc.ExitCode, string.Empty, string.Empty);
    }
}

using System.Diagnostics;
using Microsoft.Win32;

namespace ErixOpti.Core.Helpers;

public static class ServiceProbeHelper
{
    public static bool? RegistryStartIs(string serviceName, int startValue)
    {
        try
        {
            using var bk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var k = bk.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
            if (k?.GetValue("Start") is not int s)
            {
                return null;
            }

            return s == startValue;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Disabled = Start type 4 (disabled) and process not in RUNNING state when query succeeds.</summary>
    public static bool? IsServiceDisabled(string serviceName)
    {
        var reg = RegistryStartIs(serviceName, 4);
        if (reg is false)
        {
            return false;
        }

        var state = QueryScState(serviceName);
        if (state is null)
        {
            return reg;
        }

        if (state.Value == ScState.Running)
        {
            return false;
        }

        return reg is true;
    }

    private enum ScState { Running, Stopped, Other }

    private static ScState? QueryScState(string serviceName)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("sc.exe", $"query \"{serviceName}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
            if (p is null)
            {
                return null;
            }

            var stdout = p.StandardOutput.ReadToEnd();
            if (!p.WaitForExit(4000))
            {
                try
                {
                    p.Kill(entireProcessTree: true);
                }
                catch
                {
                    // ignore
                }

                return null;
            }

            if (stdout.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
            {
                return ScState.Running;
            }

            if (stdout.Contains("STOPPED", StringComparison.OrdinalIgnoreCase))
            {
                return ScState.Stopped;
            }

            return ScState.Other;
        }
        catch
        {
            return null;
        }
    }
}

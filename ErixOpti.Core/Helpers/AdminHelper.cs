using System.Diagnostics;
using System.Security.Principal;

namespace ErixOpti.Core.Helpers;

public static class AdminHelper
{
    public static bool IsRunningAsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public static bool TryRestartElevated(string arguments = "")
    {
        try
        {
            var exe = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exe))
            {
                return false;
            }

            var psi = new ProcessStartInfo(exe, arguments)
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

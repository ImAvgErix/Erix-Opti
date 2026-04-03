using Microsoft.Win32;

namespace ErixOpti.Core.Helpers;

public static class RebootDetectionHelper
{
    public static bool IsRebootPending()
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired");
            if (k?.SubKeyCount > 0)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager");
            if (k?.GetValue("PendingFileRenameOperations") is byte[] b && b.Length > 0)
            {
                return true;
            }

            if (k?.GetValue("PendingFileRenameOperations2") is byte[] b2 && b2.Length > 0)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending");
            if (k is not null)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }
}

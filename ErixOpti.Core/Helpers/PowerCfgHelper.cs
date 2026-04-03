using Microsoft.Win32;

namespace ErixOpti.Core.Helpers;

public static class PowerCfgHelper
{
    public const string SubGroupProcessor = "54533251-82be-4824-9c50-496e4846f1a7";
    public const string ProcessorMinState = "893dee8e-2bef-41e2-9c2b-bf19c0360a5d";
    public const string ProcessorMaxState = "bc5038f7-23e0-4960-96da-33abaf5935ec";
    public const string UltimatePerformanceTemplate = "e9a42b02-d5df-448d-aa00-03f14749eb61";

    public const string SubGroupDisk = "0012ee47-9041-4b5d-9b77-535fba8b1442";
    public const string DiskIdle = "6738e2c4-e8a5-4a42-b16a-e040e769756e";

    public const string SubGroupUsb = "2a737441-1930-4402-8d77-b2bebba308a3";
    public const string UsbSelectiveSuspend = "48e6b7a6-50f5-4782-a5d4-53bb8f07e226";

    public const string SubGroupPcie = "501a4d13-42af-4429-9fd1-a8218c268e20";
    public const string PcieAspm = "ee12f906-d277-404b-b6da-e5fa1a576df5";

    public const string SubGroupSleep = "238c9fa8-0aad-41ed-83f4-97be242c8f20";
    public const string Hibernate = "9d7815a6-7ee4-497e-8888-515a05f02364";

    public const string ErixPlanName = "Erix Gaming";

    public static string? GetActiveSchemeGuid()
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes");
            return k?.GetValue("ActivePowerScheme")?.ToString();
        }
        catch { return null; }
    }

    public static int? ReadAcSettingIndex(string schemeGuid, string subGroup, string setting)
    {
        try
        {
            using var bk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var k = bk.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{schemeGuid}\{subGroup}\{setting}");
            return k?.GetValue("ACSettingIndex") is int v ? v : null;
        }
        catch { return null; }
    }

    public static bool IsHibernateEnabled()
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power");
            return k?.GetValue("HibernateEnabled") is int v && v == 1;
        }
        catch { return true; }
    }

    public static bool IsActiveSchemeNamedErix()
    {
        var guid = GetActiveSchemeGuid();
        if (guid is null) return false;
        try
        {
            using var bk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var k = bk.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{guid}");
            var desc = k?.GetValue("FriendlyName")?.ToString();
            if (desc is not null && desc.Contains("Erix", StringComparison.OrdinalIgnoreCase)) return true;
            var raw = k?.GetValue("FriendlyName") as byte[];
            if (raw is not null)
            {
                var str = System.Text.Encoding.Unicode.GetString(raw).TrimEnd('\0');
                return str.Contains("Erix", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        catch { return false; }
    }
}

using Microsoft.Win32;

namespace ErixOpti.Core.Helpers;

public static class RegistryTweakHelper
{
    public static bool TryReadDword(RegistryHive hive, string subKey, string keyName, out int value)
    {
        value = 0;
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        if (key?.GetValue(keyName) is int i) { value = i; return true; }
        return false;
    }

    public static void WriteDword(RegistryHive hive, string subKey, string keyName, int value)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Cannot open registry key: {subKey}");
        key.SetValue(keyName, value, RegistryValueKind.DWord);
    }

    public static void WriteString(RegistryHive hive, string subKey, string keyName, string value)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Cannot open registry key: {subKey}");
        key.SetValue(keyName, value, RegistryValueKind.String);
    }

    public static bool TryReadString(RegistryHive hive, string subKey, string keyName, out string value)
    {
        value = "";
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        if (key?.GetValue(keyName) is string s) { value = s; return true; }
        return false;
    }

    public static void WriteBinary(RegistryHive hive, string subKey, string keyName, byte[] data)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Cannot open registry key: {subKey}");
        key.SetValue(keyName, data, RegistryValueKind.Binary);
    }

    public static bool TryReadBinary(RegistryHive hive, string subKey, string keyName, out byte[] value)
    {
        value = [];
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        if (key?.GetValue(keyName) is byte[] b) { value = b; return true; }
        return false;
    }

    public static void DeleteValue(RegistryHive hive, string subKey, string keyName, bool throwOnMissing = false)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: true);
        if (key is null) { if (throwOnMissing) throw new InvalidOperationException($"Cannot open: {subKey}"); return; }
        try { key.DeleteValue(keyName, throwOnMissing); } catch { if (throwOnMissing) throw; }
    }

    public static void WriteDefaultString(RegistryHive hive, string subKey, string value)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Cannot open: {subKey}");
        key.SetValue("", value);
    }

    public static bool DefaultStringIs(RegistryHive hive, string subKey, string expected)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        return key?.GetValue("") is string s && s == expected;
    }

    /// <summary>Enumerate PCI device instance paths whose class GUID matches.</summary>
    public static List<string> EnumeratePciDevicesByClass(string classGuid)
    {
        var results = new List<string>();
        try
        {
            using var bk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var pci = bk.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\PCI");
            if (pci is null) return results;
            foreach (var devId in pci.GetSubKeyNames())
            {
                using var devKey = pci.OpenSubKey(devId);
                if (devKey is null) continue;
                foreach (var instId in devKey.GetSubKeyNames())
                {
                    using var inst = devKey.OpenSubKey(instId);
                    var cls = inst?.GetValue("ClassGUID")?.ToString();
                    if (cls is not null && cls.Equals(classGuid, StringComparison.OrdinalIgnoreCase))
                        results.Add($@"SYSTEM\CurrentControlSet\Enum\PCI\{devId}\{instId}");
                }
            }
        }
        catch { /* enumeration best-effort */ }
        return results;
    }
}

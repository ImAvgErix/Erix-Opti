using Microsoft.Win32;

namespace ErixOpti.Core.Helpers;

public static class RegistryTweakHelper
{
    public static bool TryReadDword(RegistryHive hive, string subKey, string keyName, out int value)
    {
        value = 0;
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        if (key?.GetValue(keyName) is int i)
        {
            value = i;
            return true;
        }

        return false;
    }

    public static void WriteDword(RegistryHive hive, string subKey, string keyName, int value)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Cannot open registry key: {subKey}");
        key.SetValue(keyName, value, RegistryValueKind.DWord);
    }

    public static void DeleteValue(RegistryHive hive, string subKey, string keyName, bool throwOnMissing = false)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: true);
        if (key is null)
        {
            if (throwOnMissing)
            {
                throw new InvalidOperationException($"Cannot open registry key: {subKey}");
            }

            return;
        }

        try
        {
            key.DeleteValue(keyName, throwOnMissing);
        }
        catch
        {
            if (throwOnMissing)
            {
                throw;
            }
        }
    }

    public static void WriteDwordUser(string subKey, string keyName, int value)
    {
        WriteDword(RegistryHive.CurrentUser, subKey, keyName, value);
    }

    public static bool TryReadDwordUser(string subKey, string keyName, out int value)
    {
        return TryReadDword(RegistryHive.CurrentUser, subKey, keyName, out value);
    }
}

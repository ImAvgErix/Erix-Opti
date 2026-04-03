using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class DeviceAffinityTweaks
{
    private const string GpuClassGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}";
    private const string NicClassGuid = "{4d36e972-e325-11ce-bfc1-08002be10318}";

    public static IReadOnlyList<TweakOperation> All =>
    [
        new()
        {
            Id = "dev.gpu-msi", Name = "GPU MSI mode", Category = "Device",
            Description = "Enables Message Signaled Interrupts on GPU for lower interrupt latency.",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => CheckMsi(GpuClassGuid),
            Apply = (p, _) => { p.Report("GPU MSI mode on"); ApplyMsi(GpuClassGuid); return Task.CompletedTask; },
            Revert = (p, _) => Task.CompletedTask
        },
        new()
        {
            Id = "dev.nic-msi", Name = "NIC MSI mode", Category = "Device",
            Description = "Enables Message Signaled Interrupts on primary NIC.",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => CheckMsi(NicClassGuid),
            Apply = (p, _) => { p.Report("NIC MSI mode on"); ApplyMsi(NicClassGuid); return Task.CompletedTask; },
            Revert = (p, _) => Task.CompletedTask
        },
        new()
        {
            Id = "dev.gpu-affinity", Name = "GPU interrupt spread", Category = "Device",
            Description = "Spreads GPU MSI interrupts across all cores (DevicePolicy=5, Priority=High).",
            ShouldApply = hw => hw.IsDesktop,
            TryGetAppliedState = _ => CheckAffinity(GpuClassGuid),
            Apply = (p, _) => { p.Report("GPU affinity spread"); ApplyAffinity(GpuClassGuid); return Task.CompletedTask; },
            Revert = (p, _) => Task.CompletedTask
        },
    ];

    private static bool? CheckMsi(string classGuid)
    {
        var paths = RegistryTweakHelper.EnumeratePciDevicesByClass(classGuid);
        if (paths.Count == 0) return null;
        foreach (var path in paths)
        {
            var msiPath = $@"{path}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            if (RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, msiPath, "MSISupported", out var v) && v == 1)
                return true;
        }
        return false;
    }

    private static void ApplyMsi(string classGuid)
    {
        foreach (var path in RegistryTweakHelper.EnumeratePciDevicesByClass(classGuid))
        {
            var msiPath = $@"{path}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, msiPath, "MSISupported", 1);
            RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, msiPath, "MessageNumberLimit", 16);
        }
    }

    private static bool? CheckAffinity(string classGuid)
    {
        var paths = RegistryTweakHelper.EnumeratePciDevicesByClass(classGuid);
        if (paths.Count == 0) return null;
        foreach (var path in paths)
        {
            var afPath = $@"{path}\Device Parameters\Interrupt Management\Affinity Policy";
            if (RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, afPath, "DevicePolicy", out var dp) && dp == 5)
                return true;
        }
        return false;
    }

    private static void ApplyAffinity(string classGuid)
    {
        foreach (var path in RegistryTweakHelper.EnumeratePciDevicesByClass(classGuid))
        {
            var afPath = $@"{path}\Device Parameters\Interrupt Management\Affinity Policy";
            RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, afPath, "DevicePolicy", 5);
            RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, afPath, "DevicePriority", 3);
        }
    }
}

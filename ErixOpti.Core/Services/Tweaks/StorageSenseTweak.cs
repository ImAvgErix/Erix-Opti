using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class StorageSenseTweak : TweakBase
{
    private const string Key = @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy";

    public StorageSenseTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "storage.sense-aggressive";

    public override string Name => "Aggressive Storage Sense";

    public override string Description =>
        "Enables Storage Sense with more aggressive cleanup schedules for temporary files.";

    public override string Category => "Storage & Memory";

    public override RiskLevel Risk => RiskLevel.Medium;

    public override bool RequiresReboot => false;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(RegistryTweakHelper.TryReadDwordUser(Key, "01", out var v) && v == 1);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Configuring Storage Sense policies…");
        RegistryTweakHelper.WriteDwordUser(Key, "01", 1);
        RegistryTweakHelper.WriteDwordUser(Key, "04", 1);
        RegistryTweakHelper.WriteDwordUser(Key, "08", 1);
        RegistryTweakHelper.WriteDwordUser(Key, "256", 7);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting Storage Sense policies…");
        RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, Key, "01");
        RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, Key, "04");
        RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, Key, "08");
        RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, Key, "256");
        return Task.CompletedTask;
    }
}

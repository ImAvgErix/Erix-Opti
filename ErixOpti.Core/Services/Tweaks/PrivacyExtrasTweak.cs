using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class PrivacyExtrasTweak : TweakBase
{
    public PrivacyExtrasTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "privacy.ads-activity";

    public override string Name => "Advertising ID & activity history";

    public override string Description =>
        "Disables the advertising ID for apps and reduces activity history / cross-device sync hints.";

    public override string Category => "Privacy & Telemetry";

    public override RiskLevel Risk => RiskLevel.Low;

    public override bool RequiresReboot => false;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        _ = ct;
        var ads = RegistryTweakHelper.TryReadDwordUser(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", out var a) && a == 0;
        var act = RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", out var b) && b == 0;
        return Task.FromResult(ads && act);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Applying advertising & activity policies…");
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting advertising & activity policies…");
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1);
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed");
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities");
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana");
        return Task.CompletedTask;
    }
}

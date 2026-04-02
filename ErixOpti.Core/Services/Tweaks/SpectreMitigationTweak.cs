using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class SpectreMitigationTweak : TweakBase
{
    private const string Key = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";

    public SpectreMitigationTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "advanced.spectre-mitigations";

    public override string Name => "Adjust speculative execution mitigations";

    public override string Description =>
        "EXPERIMENTAL: Toggles Memory Management mitigation overrides. This can significantly reduce security against " +
        "Spectre-class attacks and may violate organizational policy. Fully reversible, but extremely risky.";

    public override string Category => "Advanced / High Risk";

    public override RiskLevel Risk => RiskLevel.High;

    public override bool RequiresReboot => true;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(
            RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, "FeatureSettingsOverride", out var a) &&
            RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, "FeatureSettingsOverrideMask", out var b) &&
            a == 3 &&
            b == 3);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Writing mitigation override values (HIGH RISK)…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "FeatureSettingsOverride", 3);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "FeatureSettingsOverrideMask", 3);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Removing mitigation overrides…");
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, Key, "FeatureSettingsOverride");
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, Key, "FeatureSettingsOverrideMask");
        return Task.CompletedTask;
    }
}

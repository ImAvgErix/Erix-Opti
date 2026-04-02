using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class TelemetryTweak : TweakBase
{
    private const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";

    public TelemetryTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "privacy.telemetry";

    public override string Name => "Reduce Windows telemetry (policy)";

    public override string Description =>
        "Sets enterprise-style policy keys to minimize diagnostic data collection where supported.";

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
        return Task.FromResult(RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, PolicyKey, "AllowTelemetry", out var v) && v == 0);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Applying telemetry policy keys…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, PolicyKey, "AllowTelemetry", 0);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, PolicyKey, "MaxTelemetryAllowed", 1);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting telemetry policy keys…");
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "AllowTelemetry");
        RegistryTweakHelper.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "MaxTelemetryAllowed");
        return Task.CompletedTask;
    }
}

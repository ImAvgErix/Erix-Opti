using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class HvciTweak : TweakBase
{
    private const string Key = @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity";

    public HvciTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "advanced.hvci-disable";

    public override string Name => "Disable Memory Integrity (HVCI)";

    public override string Description =>
        "Turns off Hypervisor-Enforced Code Integrity. This weakens kernel exploit mitigations and may be required for some " +
        "legacy drivers — use only if you understand the security trade-offs.";

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
        return Task.FromResult(RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, "Enabled", out var v) && v == 0);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Disabling HVCI policy flags…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "Enabled", 0);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "WasEnabledByGroupPolicy", 0);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Re-enabling HVCI policy flags…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "Enabled", 1);
        return Task.CompletedTask;
    }
}

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class SystemResponsivenessTweak : TweakBase
{
    private const string Key = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";

    public SystemResponsivenessTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "perf.system-responsiveness";

    public override string Name => "Foreground boost (SystemResponsiveness)";

    public override string Description =>
        "Lowers SystemResponsiveness to prioritize foreground workloads (games) over background tasks.";

    public override string Category => "Performance & Gaming";

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
        return Task.FromResult(RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, "SystemResponsiveness", out var v) && v == 10);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Setting SystemResponsiveness=10…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "SystemResponsiveness", 10);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Restoring SystemResponsiveness default…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "SystemResponsiveness", 20);
        return Task.CompletedTask;
    }
}

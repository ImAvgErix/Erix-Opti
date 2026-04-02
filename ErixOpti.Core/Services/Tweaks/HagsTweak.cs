using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class HagsTweak : TweakBase
{
    private const string Key = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
    private const string ValueName = "HwSchMode";

    public HagsTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "perf.hags";

    public override string Name => "Hardware-accelerated GPU scheduling (HAGS)";

    public override string Description =>
        "Enables the Windows GPU scheduler mode that can reduce latency on supported drivers and hardware.";

    public override string Category => "Performance & Gaming";

    public override RiskLevel Risk => RiskLevel.Low;

    public override bool RequiresReboot => true;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        _ = ct;
        if (RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, ValueName, out var v))
        {
            return Task.FromResult(v == 2);
        }

        return Task.FromResult(false);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Enabling HAGS (HwSchMode=2)…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, ValueName, 2);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting HAGS…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, ValueName, 1);
        return Task.CompletedTask;
    }
}

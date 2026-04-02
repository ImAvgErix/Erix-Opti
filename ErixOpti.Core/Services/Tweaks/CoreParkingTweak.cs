using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class CoreParkingTweak : TweakBase
{
    public CoreParkingTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "perf.core-parking";

    public override string Name => "Raise minimum processor state (reduce parking)";

    public override string Description =>
        "Increases minimum CPU performance state on AC power. On laptops, avoid when on battery — this can increase heat and drain.";

    public override string Category => "Performance & Gaming";

    public override RiskLevel Risk => RiskLevel.Medium;

    public override bool RequiresReboot => false;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        var h = Hardware.Current;
        if (h.FormFactor == FormFactor.Laptop && h.PowerSource == PowerSource.Battery)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(h.FormFactor == FormFactor.Desktop || h.PowerSource == PowerSource.Ac);
    }

    public override async Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        var (code, stdout, _) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-query SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState}",
            runElevated: false,
            progress: null,
            ct).ConfigureAwait(false);

        _ = code;
        return stdout.Contains("100%", StringComparison.Ordinal);
    }

    public override async Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        progress.Report("Setting minimum processor state to 100% (AC/DC)…");
        var (c1, _, e1) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 100",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c1 != 0)
        {
            throw new InvalidOperationException(e1);
        }

        var (c2, _, e2) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-setdcvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 100",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c2 != 0)
        {
            throw new InvalidOperationException(e2);
        }

        var (c3, _, e3) = await ProcessRunner.RunAsync(
            "powercfg",
            "-setactive SCHEME_CURRENT",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c3 != 0)
        {
            throw new InvalidOperationException(e3);
        }
    }

    public override async Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        progress.Report("Restoring default minimum processor state…");
        var (c1, _, e1) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-setacvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 5",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c1 != 0)
        {
            throw new InvalidOperationException(e1);
        }

        var (c2, _, e2) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-setdcvalueindex SCHEME_CURRENT {PowerCfgHelper.SubGroupProcessor} {PowerCfgHelper.ProcessorMinState} 5",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c2 != 0)
        {
            throw new InvalidOperationException(e2);
        }

        var (c3, _, e3) = await ProcessRunner.RunAsync(
            "powercfg",
            "-setactive SCHEME_CURRENT",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c3 != 0)
        {
            throw new InvalidOperationException(e3);
        }
    }
}

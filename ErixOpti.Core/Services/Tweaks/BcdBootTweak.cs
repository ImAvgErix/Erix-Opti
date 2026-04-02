using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class BcdBootTweak : TweakBase
{
    public BcdBootTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "advanced.bcd-boot";

    public override string Name => "BCD: disable boot UX animation";

    public override string Description =>
        "Runs bcdedit to disable the graphical boot animation for slightly faster POST-to-desktop transitions.";

    public override string Category => "Advanced / High Risk";

    public override RiskLevel Risk => RiskLevel.High;

    public override bool RequiresReboot => true;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override async Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        var (code, stdout, _) = await ProcessRunner.RunAsync(
            "bcdedit",
            "/enum {current}",
            runElevated: false,
            progress: null,
            ct).ConfigureAwait(false);

        _ = code;
        return stdout.Contains("bootux", StringComparison.OrdinalIgnoreCase) &&
               stdout.Contains("disabled", StringComparison.OrdinalIgnoreCase);
    }

    public override async Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        var (code, _, err) = await ProcessRunner.RunAsync(
            "bcdedit",
            "/set {default} bootux disabled",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (code != 0)
        {
            throw new InvalidOperationException(err);
        }
    }

    public override async Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        var (code, _, err) = await ProcessRunner.RunAsync(
            "bcdedit",
            "/deletevalue {default} bootux",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (code != 0)
        {
            throw new InvalidOperationException(err);
        }
    }
}

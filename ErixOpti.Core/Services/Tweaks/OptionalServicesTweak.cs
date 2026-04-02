using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class OptionalServicesTweak : TweakBase
{
    private static readonly string[] Services =
    [
        "DiagTrack",
        "dmwappushservice",
        "WSearch"
    ];

    public OptionalServicesTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "svc.optional-disable";

    public override string Name => "Disable selected background services";

    public override string Description =>
        "Disables a conservative set of non-essential services (DiagTrack, WaaS Medic push, Windows Search indexing). " +
        "Review each service — disabling Search stops file indexing.";

    public override string Category => "Services & Background";

    public override RiskLevel Risk => RiskLevel.Medium;

    public override bool RequiresReboot => false;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override async Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        foreach (var svc in Services)
        {
            var (code, stdout, _) = await ProcessRunner.RunAsync(
                "sc.exe",
                $"qc {svc}",
                runElevated: false,
                progress: null,
                ct).ConfigureAwait(false);

            if (code != 0 || !stdout.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public override async Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        foreach (var svc in Services)
        {
            progress.Report($"Disabling service {svc}…");
            var (code, _, err) = await ProcessRunner.RunAsync(
                "sc.exe",
                $"config {svc} start= disabled",
                runElevated: false,
                progress,
                ct).ConfigureAwait(false);

            if (code != 0)
            {
                throw new InvalidOperationException($"sc config failed for {svc}: {err}");
            }

            var (c2, _, e2) = await ProcessRunner.RunAsync(
                "sc.exe",
                $"stop {svc}",
                runElevated: false,
                progress,
                ct).ConfigureAwait(false);

            _ = c2;
            _ = e2;
        }
    }

    public override async Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        foreach (var svc in Services)
        {
            progress.Report($"Restoring service {svc}…");
            var start = svc == "WSearch" ? "delayed-auto" : "demand";
            var (code, _, err) = await ProcessRunner.RunAsync(
                "sc.exe",
                $"config {svc} start= {start}",
                runElevated: false,
                progress,
                ct).ConfigureAwait(false);

            if (code != 0)
            {
                throw new InvalidOperationException($"sc config failed for {svc}: {err}");
            }
        }
    }
}

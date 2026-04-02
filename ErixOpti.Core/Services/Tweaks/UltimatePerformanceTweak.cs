using System.Text.RegularExpressions;
using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class UltimatePerformanceTweak : TweakBase
{
    private static readonly Guid Balanced = Guid.Parse("381b4222-6948-41d0-8b5a-4c4a4b4f4b4d");

    private readonly string _stateFile;

    public UltimatePerformanceTweak(IHardwareService hardware) : base(hardware)
    {
        _stateFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ErixOpti",
            "ultimate-performance-guid.txt");
    }

    public override string Id => "perf.ultimate-power";

    public override string Name => "Ultimate Performance power plan";

    public override string Description =>
        "Duplicates and activates the Ultimate Performance power scheme for maximum CPU/GPU power limits on AC power.";

    public override string Category => "Performance & Gaming";

    public override RiskLevel Risk => RiskLevel.Medium;

    public override bool RequiresReboot => false;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(true);
    }

    public override async Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        if (!File.Exists(_stateFile))
        {
            return false;
        }

        var guidText = await File.ReadAllTextAsync(_stateFile, ct).ConfigureAwait(false);
        if (!Guid.TryParse(guidText.Trim(), out var guid))
        {
            return false;
        }

        var (code, stdout, _) = await ProcessRunner.RunAsync(
            "powercfg",
            "/getactivescheme",
            runElevated: false,
            progress: null,
            ct).ConfigureAwait(false);

        _ = code;
        return stdout.Contains(guid.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public override async Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        progress.Report("Duplicating Ultimate Performance plan…");
        var (code, stdout, stderr) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-duplicatescheme {PowerCfgHelper.UltimatePerformanceTemplate} ErixOpti-Ultimate",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (code != 0)
        {
            throw new InvalidOperationException(stderr);
        }

        var match = Regex.Match(stdout, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        if (!match.Success || !Guid.TryParse(match.Value, out var guid))
        {
            throw new InvalidOperationException("Could not parse duplicated power scheme GUID.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_stateFile)!);
        await File.WriteAllTextAsync(_stateFile, guid.ToString(), ct).ConfigureAwait(false);

        progress.Report("Activating Ultimate Performance…");
        var (c2, _, e2) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-setactive {guid}",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (c2 != 0)
        {
            throw new InvalidOperationException(e2);
        }
    }

    public override async Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        progress.Report("Restoring Balanced power plan…");
        var (code, _, err) = await ProcessRunner.RunAsync(
            "powercfg",
            $"-setactive {Balanced}",
            runElevated: false,
            progress,
            ct).ConfigureAwait(false);

        if (code != 0)
        {
            throw new InvalidOperationException(err);
        }

        if (File.Exists(_stateFile))
        {
            File.Delete(_stateFile);
        }
    }
}

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class GameModeTweak : TweakBase
{
    private const string Key = @"Software\Microsoft\GameBar";

    public GameModeTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "perf.gamemode";

    public override string Name => "Game Mode & windowed optimizations";

    public override string Description =>
        "Turns on Windows Game Mode and related scheduling hints for foreground games.";

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
        var ok1 = RegistryTweakHelper.TryReadDwordUser(Key, "AllowAutoGameMode", out var a) && a == 1;
        var ok2 = RegistryTweakHelper.TryReadDwordUser(Key, "AutoGameModeEnabled", out var b) && b == 1;
        return Task.FromResult(ok1 && ok2);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Enabling Game Mode settings…");
        RegistryTweakHelper.WriteDwordUser(Key, "AllowAutoGameMode", 1);
        RegistryTweakHelper.WriteDwordUser(Key, "AutoGameModeEnabled", 1);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\GameBar", "GameModeEnabled", 1);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting Game Mode settings…");
        RegistryTweakHelper.WriteDwordUser(Key, "AllowAutoGameMode", 0);
        RegistryTweakHelper.WriteDwordUser(Key, "AutoGameModeEnabled", 0);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\GameBar", "GameModeEnabled", 0);
        return Task.CompletedTask;
    }
}

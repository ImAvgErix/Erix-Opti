using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class PrefetchSsdTweak : TweakBase
{
    private const string Key = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters";

    public PrefetchSsdTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "storage.prefetch-ssd";

    public override string Name => "Disable Prefetch/Superfetch (SSD only)";

    public override string Description =>
        "Disables the Prefetcher when an SSD is detected — not recommended for HDD boot volumes.";

    public override string Category => "Storage & Memory";

    public override RiskLevel Risk => RiskLevel.Medium;

    public override bool RequiresReboot => true;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(Hardware.Current.HasSsdBootVolume);
    }

    public override Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, "EnablePrefetcher", out var v) && v == 0);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Disabling Prefetch/Superfetch…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "EnablePrefetcher", 0);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "EnableSuperfetch", 0);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Restoring Prefetch defaults…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "EnablePrefetcher", 3);
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "EnableSuperfetch", 3);
        return Task.CompletedTask;
    }
}

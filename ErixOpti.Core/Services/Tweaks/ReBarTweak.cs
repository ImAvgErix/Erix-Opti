using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class ReBarTweak : TweakBase
{
    private const string Key = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";

    public ReBarTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "perf.rebar";

    public override string Name => "Resizable BAR readiness (OS hints)";

    public override string Description =>
        "Sets OS-side ReBAR-related flags where supported. Full ReBAR still requires BIOS and GPU support; " +
        "NVIDIA/AMD/iGPU policies differ — verify in your UEFI and GPU control panel.";

    public override string Category => "Performance & Gaming";

    public override RiskLevel Risk => RiskLevel.Medium;

    public override bool RequiresReboot => true;

    public override Task<bool> IsApplicableAsync(CancellationToken ct)
    {
        _ = ct;
        var v = Hardware.Current.PrimaryGpuVendor;
        return Task.FromResult(v is GpuVendor.Nvidia or GpuVendor.Amd or GpuVendor.Intel);
    }

    public override Task<bool> IsAppliedAsync(CancellationToken ct)
    {
        _ = ct;
        if (RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, Key, "ReBarFirmwareSupport", out var v))
        {
            return Task.FromResult(v == 1);
        }

        return Task.FromResult(false);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Setting ReBAR firmware support hint…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "ReBarFirmwareSupport", 1);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting ReBAR hint…");
        RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, Key, "ReBarFirmwareSupport", 0);
        return Task.CompletedTask;
    }
}

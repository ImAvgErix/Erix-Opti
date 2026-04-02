using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public sealed class VisualEffectsTweak : TweakBase
{
    public VisualEffectsTweak(IHardwareService hardware) : base(hardware)
    {
    }

    public override string Id => "visual.effects";

    public override string Name => "Reduce visual effects";

    public override string Description =>
        "Disables many window/animation flourishes to reduce compositor overhead and input latency.";

    public override string Category => "Visual & System";

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
        var anim = RegistryTweakHelper.TryReadDwordUser(@"Control Panel\Desktop\WindowMetrics", "MinAnimate", out var a) && a == 0;
        var fx = RegistryTweakHelper.TryReadDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", out var b) && b == 2;
        return Task.FromResult(anim && fx);
    }

    public override Task ApplyAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Applying reduced visual effects…");
        RegistryTweakHelper.WriteDwordUser(@"Control Panel\Desktop\WindowMetrics", "MinAnimate", 0);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 0);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 0);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2);
        return Task.CompletedTask;
    }

    public override Task RevertAsync(IProgress<string> progress, CancellationToken ct)
    {
        _ = ct;
        progress.Report("Reverting visual effects…");
        RegistryTweakHelper.WriteDwordUser(@"Control Panel\Desktop\WindowMetrics", "MinAnimate", 1);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 1);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 1);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 1);
        RegistryTweakHelper.WriteDwordUser(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1);
        RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting");
        return Task.CompletedTask;
    }
}

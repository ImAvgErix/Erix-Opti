using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class RegistryTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        D("reg.mouse-queue", "Mouse queue 32", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "MouseDataQueueSize", 32, 100),
        D("reg.kb-queue", "Keyboard queue 24", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "KeyboardDataQueueSize", 24, 100),
        D("reg.mouse-nolazy", "Mouse no lazy mode", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "NoLazyMode", 1, 0),
        D("reg.kb-nolazy", "Keyboard no lazy mode", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "NoLazyMode", 1, 0),
        D("reg.sys-resp", "SystemResponsiveness 0", "System", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0, 20),
        D("reg.net-throttle", "Disable network throttle", "Network", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), 10),
        new()
        {
            Id = "reg.priority-sep", Name = "Win32PrioritySeparation", Category = "System",
            ShouldApply = _ => true,
            Apply = (p, _) => { var v = HwRef.Hw?.IsAmdCpu == true ? 0x28 : 0x26; p.Report($"PrioritySeparation=0x{v:X2}"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", v); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 0x02); return Task.CompletedTask; }
        },
        D("reg.fg-lock", "ForegroundLockTimeout 0", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "ForegroundLockTimeout", 0, 200000),
        D("reg.hooks-timeout", "LowLevelHooksTimeout 0", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "LowLevelHooksTimeout", 0, 300),
        D("reg.menu-delay", "MenuShowDelay 0", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", 0, 400),
        D("reg.kill-svc", "WaitToKillService 2000", "System", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control", "WaitToKillServiceTimeout", 2000, 5000),
        D("reg.hung-app", "HungAppTimeout 2000", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "HungAppTimeout", 2000, 5000),
        D("reg.large-cache", "LargeSystemCache", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 1, 0),
        D("reg.no-paging-exec", "DisablePagingExecutive", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1, 0),
        D("reg.nonpaged-pool", "NonPagedPoolSize max", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "NonPagedPoolSize", unchecked((int)0xFFFFFFFF), 0),
        D("reg.gamemode-off", "Game Mode off", "Gaming", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 0, 1),
        D("reg.gamemode-off2", "AllowAutoGameMode off", "Gaming", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 0, 1),
        D("reg.gamedvr-off", "GameDVR off", "Gaming", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0, 1),
        D("reg.appcapture", "AppCapture off", "Gaming", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AppCaptureEnabled", 0, 1),
        D("reg.telemetry", "AllowTelemetry 0", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, 1),
        D("reg.toast-off", "Toast off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 0, 1),
        D("reg.notif-off", "NotifCenter off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, 0),
        D("reg.adid-off", "Ad ID off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1),
        D("reg.activity-off", "Activity feed off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, 1),
        D("reg.cortana-off", "Cortana off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, 1),
        D("reg.bing-off", "Bing search off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, 0),
        D("reg.content-off", "Content delivery off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, 1),
        D("reg.show-ext", "Show file extensions", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, 1),
        D("reg.show-hidden", "Show hidden files", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, 2),
        D("reg.open-thispc", "Open to This PC", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, 0),
        D("reg.no-explorer-ads", "No Explorer ads", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, 1),
        D("reg.bg-apps-off", "Background apps off", "System", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, 0),
        D("reg.prefetch-off", "Prefetch off (SSD)", "Storage", hw => hw.HasSsdBootVolume, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 0, 3),
        D("reg.superfetch-off", "Superfetch off (SSD)", "Storage", hw => hw.HasSsdBootVolume, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 0, 3),
    ];

    private static TweakOperation D(string id, string name, string cat, Func<HardwareInfo, bool> cond, RegistryHive hive, string key, string val, int apply, int revert) => new()
    {
        Id = id, Name = name, Category = cat, ShouldApply = cond,
        Apply = (p, _) => { p.Report(name); RegistryTweakHelper.WriteDword(hive, key, val, apply); return Task.CompletedTask; },
        Revert = (p, _) => { p.Report($"Revert {name}"); if (revert == -1) RegistryTweakHelper.DeleteValue(hive, key, val); else RegistryTweakHelper.WriteDword(hive, key, val, revert); return Task.CompletedTask; }
    };
}

internal static class HwRef { public static HardwareInfo? Hw { get; set; } }

using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class RegistryTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        // ── Input ──
        D("reg.mouse-queue", "Mouse queue 32", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "MouseDataQueueSize", 32, 100),
        D("reg.kb-queue", "Keyboard queue 24", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "KeyboardDataQueueSize", 24, 100),
        D("reg.mouse-nolazy", "Mouse no lazy mode", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "NoLazyMode", 1, 0),
        D("reg.kb-nolazy", "Keyboard no lazy mode", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "NoLazyMode", 1, 0),
        D("reg.mouse-freq", "Mouse sample rate max", "Input", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "SampleRate", 250, 100),

        // ── System responsiveness ──
        D("reg.sys-resp", "SystemResponsiveness 0", "System", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0, 20),
        D("reg.net-throttle", "Disable network throttle", "Network", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), 10),
        new()
        {
            Id = "reg.priority-sep", Name = "Win32PrioritySeparation", Category = "System",
            ShouldApply = _ => true,
            TryGetAppliedState = hw =>
            {
                if (!RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", out var cur))
                    return false;
                var want = hw.IsAmdCpu ? 0x28 : 0x26;
                return cur == want;
            },
            Apply = (p, _) => { var v = HwRef.Hw?.IsAmdCpu == true ? 0x28 : 0x26; p.Report($"PrioritySeparation=0x{v:X2}"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", v); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 0x02); return Task.CompletedTask; }
        },
        D("reg.fg-lock", "ForegroundLockTimeout 0", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "ForegroundLockTimeout", 0, 200000),
        D("reg.hooks-timeout", "LowLevelHooksTimeout 0", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "LowLevelHooksTimeout", 0, 300),
        D("reg.menu-delay", "MenuShowDelay 0", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", 0, 400),
        D("reg.kill-svc", "WaitToKillService 2000", "System", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control", "WaitToKillServiceTimeout", 2000, 5000),
        D("reg.hung-app", "HungAppTimeout 2000", "System", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "HungAppTimeout", 2000, 5000),
        D("reg.boot-delay", "Disable startup delay", "System", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0, -1),
        D("reg.bg-apps-off", "Background apps off", "System", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, 0),
        D("reg.mmcss-gaming", "MMCSS gaming priority", "System", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6, 2),
        D("reg.mmcss-sched", "MMCSS scheduling Games", "System", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", 2, 0),
        D("reg.mmcss-sfio", "MMCSS SFIO priority Games", "System", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", 2, 0),
        D("reg.svc-host-split", "SvcHostSplitThreshold", "System", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control", "SvcHostSplitThresholdInKB", 0x03800000, 380000),
        D("reg.auto-reboot-off", "No auto reboot for updates", "System", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, 0),

        // ── Memory ──
        D("reg.large-cache", "LargeSystemCache", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 1, 0),
        D("reg.no-paging-exec", "DisablePagingExecutive", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1, 0),
        D("reg.nonpaged-pool", "NonPagedPoolSize max", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "NonPagedPoolSize", unchecked((int)0xFFFFFFFF), 0),
        D("reg.io-page-lock-limit", "IoPageLockLimit", "Memory", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "IoPageLockLimit", 0x10000, 0),

        // ── Gaming ──
        D("reg.gamemode-off", "Game Mode off", "Gaming", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 0, 1),
        D("reg.gamemode-off2", "AllowAutoGameMode off", "Gaming", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 0, 1),
        D("reg.gamedvr-off", "GameDVR off", "Gaming", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0, 1),
        D("reg.appcapture", "AppCapture off", "Gaming", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AppCaptureEnabled", 0, 1),
        D("reg.fso-global", "Fullscreen optimizations off", "Gaming", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 2, 0),
        D("reg.fso-dsc", "Fullscreen DSC compat off", "Gaming", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 1, 0),
        D("reg.fso-hgm", "Fullscreen HGM off", "Gaming", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 1, 0),
        D("reg.gpu-perf-pref", "GPU high performance preference", "Gaming", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\DirectX\UserGpuPreferences", "DirectXUserGlobalSettings", 2, 0),
        D("reg.gamebar-off", "Game Bar off", "Gaming", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, 1),

        // ── Privacy ──
        D("reg.telemetry", "AllowTelemetry 0", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, 1),
        D("reg.toast-off", "Toast off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 0, 1),
        D("reg.notif-off", "NotifCenter off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, 0),
        D("reg.adid-off", "Ad ID off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1),
        D("reg.activity-off", "Activity feed off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, 1),
        D("reg.cortana-off", "Cortana off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, 1),
        D("reg.bing-off", "Bing search off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, 0),
        D("reg.content-off", "Content delivery off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, 1),
        D("reg.location-off", "Location tracking off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1, 0),
        D("reg.feedback-off", "Feedback notifications off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, -1),
        D("reg.tips-off", "Tips and suggestions off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, 1),
        D("reg.lockscreen-tips", "Lock screen tips off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, 1),
        D("reg.defender-sample", "Defender auto-sample off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SubmitSamplesConsent", 2, 1),
        D("reg.diag-off", "Diagnostic data off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack", "ShowedToastAtLevel", 1, 0),
        D("reg.ink-off", "Windows Ink Workspace off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsInkWorkspace", "AllowWindowsInkWorkspace", 0, 1),
        D("reg.handwriting-off", "Handwriting telemetry off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, 0),
        D("reg.cdp-off", "Connected Devices off", "Privacy", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableCdp", 0, 1),
        D("reg.tailored-off", "Tailored experiences off", "Privacy", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0, 1),

        // ── AI Removal ──
        D("reg.copilot-off", "Copilot off", "AI Removal", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0),
        D("reg.copilot-policy", "Copilot policy off", "AI Removal", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0),
        D("reg.recall-off", "Recall off", "AI Removal", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, 0),
        D("reg.recall-user", "Recall snapshots off", "AI Removal", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DisableAIDataAnalysis", 1, 0),
        D("reg.ai-companion", "AI companion off", "AI Removal", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowAICompanion", 0, 1),
        D("reg.search-ai", "Search AI off", "AI Removal", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsAADCloudSearchEnabled", 0, 1),
        D("reg.search-ai2", "Search highlights off", "AI Removal", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0, 1),

        // ── Dark Mode ──
        D("reg.dark-apps", "Dark mode apps", "Dark Mode", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, 1),
        D("reg.dark-system", "Dark mode system", "Dark Mode", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, 1),
        D("reg.dark-transparency", "Transparency effects on", "Dark Mode", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1, 0),

        // ── Explorer / Visual ──
        D("reg.show-ext", "Show file extensions", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, 1),
        D("reg.show-hidden", "Show hidden files", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, 2),
        D("reg.open-thispc", "Open to This PC", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, 0),
        D("reg.no-explorer-ads", "No Explorer ads", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, 1),
        D("reg.widgets-off", "Widgets off", "Explorer", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, 1),
        D("reg.no-anim", "Disable window animations", "Visual", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate", 0, 1),
        D("reg.drag-height", "Reduce drag detection", "Visual", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "DragHeight", 2, 4),
        D("reg.drag-width", "Reduce drag detection width", "Visual", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "DragWidth", 2, 4),
        D("reg.smooth-scroll", "Disable smooth scrolling", "Visual", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "SmoothScroll", 0, 1),
        D("reg.taskbar-search", "Taskbar search icon only", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, 2),
        D("reg.chat-off", "Chat icon off", "Explorer", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, 1),

        // ── Storage / NTFS ──
        D("reg.prefetch-off", "Prefetch off (SSD)", "Storage", hw => hw.HasSsdBootVolume, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 0, 3),
        D("reg.superfetch-off", "Superfetch off (SSD)", "Storage", hw => hw.HasSsdBootVolume, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 0, 3),
        D("reg.ntfs-lastaccess", "NTFS last access off", "Storage", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate", 1, 0),
        D("reg.ntfs-8dot3", "NTFS 8.3 naming off", "Storage", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisable8dot3NameCreation", 1, 0),
        D("reg.ntfs-memuse", "NTFS memory usage high", "Storage", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsMemoryUsage", 2, 1),
    ];

    private static TweakOperation D(string id, string name, string cat, Func<HardwareInfo, bool> cond, RegistryHive hive, string key, string val, int apply, int revert) => new()
    {
        Id = id, Name = name, Category = cat, ShouldApply = cond,
        TryGetAppliedState = _ =>
        {
            if (!RegistryTweakHelper.TryReadDword(hive, key, val, out var cur))
                return false;
            return cur == apply;
        },
        Apply = (p, _) => { p.Report(name); RegistryTweakHelper.WriteDword(hive, key, val, apply); return Task.CompletedTask; },
        Revert = (p, _) => { p.Report($"Revert {name}"); if (revert == -1) RegistryTweakHelper.DeleteValue(hive, key, val); else RegistryTweakHelper.WriteDword(hive, key, val, revert); return Task.CompletedTask; }
    };
}

internal static class HwRef { public static HardwareInfo? Hw { get; set; } }

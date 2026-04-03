using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class RegistryTweaks
{
    public static IReadOnlyList<TweakOperation> All =>
    [
        // ── Input ──
        D("reg.mouse-queue", "Mouse queue 32", "Input", "Reduces mouse input buffer for faster response.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "MouseDataQueueSize", 32, 100),
        D("reg.kb-queue", "Keyboard queue 24", "Input", "Reduces keyboard input buffer.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "KeyboardDataQueueSize", 24, 100),
        D("reg.mouse-nolazy", "Mouse no lazy mode", "Input", "Disables lazy processing for mouse driver.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "NoLazyMode", 1, 0),
        D("reg.kb-nolazy", "Keyboard no lazy mode", "Input", "Disables lazy processing for keyboard driver.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "NoLazyMode", 1, 0),
        D("reg.mouse-freq", "Mouse sample rate max", "Input", "Sets mouse sample rate to 250 Hz.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "SampleRate", 250, 100),
        new()
        {
            Id = "reg.mouse-accel", Name = "Mouse acceleration off", Category = "Input",
            Description = "Disables mouse acceleration for 1:1 tracking.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ =>
            {
                return RegistryTweakHelper.TryReadString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", out var s) && s == "0"
                    && RegistryTweakHelper.TryReadString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", out var t1) && t1 == "0"
                    && RegistryTweakHelper.TryReadString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", out var t2) && t2 == "0";
            },
            Apply = (p, _) => { p.Report("Mouse acceleration off"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "0"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "0"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "0"); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "1"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "6"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "10"); return Task.CompletedTask; }
        },

        // ── System ──
        D("reg.sys-resp", "SystemResponsiveness 0", "System", "Maximum foreground priority for multimedia.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0, 20),
        D("reg.net-throttle", "Network throttle off", "Network", "Disables network throttling index.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), 10),
        new()
        {
            Id = "reg.priority-sep", Name = "Win32PrioritySeparation", Category = "System",
            Description = "CPU scheduling: AMD=0x28, Intel=0x26 for optimal foreground priority.",
            ShouldApply = _ => true,
            TryGetAppliedState = hw => { if (!RegistryTweakHelper.TryReadDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", out var c)) return false; return c == (hw.IsAmdCpu ? 0x28 : 0x26); },
            Apply = (p, _) => { var v = HwRef.Hw?.IsAmdCpu == true ? 0x28 : 0x26; p.Report($"PrioritySeparation=0x{v:X2}"); RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", v); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 0x02); return Task.CompletedTask; }
        },
        D("reg.fg-lock", "ForegroundLockTimeout 0", "System", "Instant foreground window focus.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "ForegroundLockTimeout", 0, 200000),
        D("reg.hooks-timeout", "LowLevelHooksTimeout 0", "System", "Reduces low-level hook timeout.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "LowLevelHooksTimeout", 0, 300),
        D("reg.menu-delay", "MenuShowDelay 0", "System", "Removes menu display delay.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", 0, 400),
        D("reg.kill-svc", "WaitToKillService 2000", "System", "Faster service shutdown.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control", "WaitToKillServiceTimeout", 2000, 5000),
        D("reg.hung-app", "HungAppTimeout 2000", "System", "Faster hung-app detection.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "HungAppTimeout", 2000, 5000),
        D("reg.kill-app", "WaitToKillAppTimeout 2000", "System", "Faster shutdown app termination.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "WaitToKillAppTimeout", 2000, 20000),
        D("reg.boot-delay", "Startup delay off", "System", "Removes artificial startup program delay.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0, -1),
        D("reg.bg-apps-off", "Background apps off", "System", "Disables UWP background access globally.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, 0),
        D("reg.mmcss-gaming", "MMCSS gaming priority", "System", "Elevates MMCSS Games task priority.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6, 2),
        D("reg.mmcss-sched", "MMCSS scheduling high", "System", "MMCSS Games scheduling category = high.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", 2, 0),
        D("reg.mmcss-sfio", "MMCSS SFIO priority", "System", "Elevates MMCSS scheduled I/O priority.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", 2, 0),
        D("reg.svc-host-split", "SvcHostSplitThreshold", "System", "Reduces service host splitting (high-RAM).", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control", "SvcHostSplitThresholdInKB", 0x03800000, 380000),
        D("reg.auto-reboot-off", "No auto-reboot updates", "System", "Prevents forced restart for updates.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, 0),
        D("reg.auto-maint-off", "Automatic maintenance off", "System", "Disables scheduled maintenance tasks.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1, 0),
        D("reg.error-report", "Error reporting off", "System", "Disables Windows Error Reporting.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, 0),
        D("reg.fth-off", "Fault tolerant heap off", "System", "Disables FTH overhead on crashed apps.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\FTH", "Enabled", 0, 1),
        D("reg.serialize-timer", "Serialize timer expiration", "System", "Distributes timer DPCs across all cores.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "SerializeTimerExpiration", 1, 0),
        D("reg.autorun-off", "Autorun off", "System", "Disables auto-execution on all drive types.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 255, 0),

        // ── Memory ──
        D("reg.large-cache", "LargeSystemCache", "Memory", "Increases file system cache (high-RAM).", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 1, 0),
        D("reg.no-paging-exec", "DisablePagingExecutive", "Memory", "Keeps kernel code in RAM.", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1, 0),
        D("reg.nonpaged-pool", "NonPagedPoolSize max", "Memory", "Maximizes non-paged pool allocation.", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "NonPagedPoolSize", unchecked((int)0xFFFFFFFF), 0),
        D("reg.io-page-lock", "IoPageLockLimit", "Memory", "Increases I/O page lock limit.", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "IoPageLockLimit", 0x10000, 0),

        // ── Gaming ──
        D("reg.gamemode-off", "Game Mode off", "Gaming", "Disables Game Mode (can cause micro-stutters).", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 0, 1),
        D("reg.gamemode-off2", "AllowAutoGameMode off", "Gaming", "Prevents automatic Game Mode.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 0, 1),
        D("reg.gamedvr-off", "GameDVR off", "Gaming", "Disables background game recording.", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0, 1),
        D("reg.appcapture", "AppCapture off", "Gaming", "Disables screen capture policy.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AppCaptureEnabled", 0, 1),
        D("reg.fso-global", "FSO off", "Gaming", "Disables fullscreen optimizations (reduces input lag).", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 2, 0),
        D("reg.fso-dsc", "FSO DSC compat off", "Gaming", "Disables DXGI fullscreen compatibility.", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 1, 0),
        D("reg.fso-hgm", "FSO HGM off", "Gaming", "Disables honor-user fullscreen override.", _ => true, RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 1, 0),
        D("reg.gpu-perf-pref", "GPU high perf preference", "Gaming", "Sets DirectX GPU preference to high performance.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\DirectX\UserGpuPreferences", "DirectXUserGlobalSettings", 2, 0),
        D("reg.gamebar-off", "Game Bar off", "Gaming", "Disables Xbox Game Bar overlay.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, 1),

        // ── Privacy ──
        D("reg.telemetry", "Telemetry off", "Privacy", "Disables Windows telemetry.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, 1),
        D("reg.adid-off", "Ad ID off", "Privacy", "Disables advertising identifier.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1),
        D("reg.activity-off", "Activity feed off", "Privacy", "Disables activity history.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, 1),
        D("reg.cortana-off", "Cortana off", "Privacy", "Disables Cortana.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, 1),
        D("reg.bing-off", "Bing search off", "Privacy", "Removes Bing from Start search.", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, 0),
        D("reg.content-off", "Content delivery off", "Privacy", "Disables suggested apps.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, 1),
        D("reg.location-off", "Location off", "Privacy", "Disables system location tracking.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1, 0),
        D("reg.feedback-off", "Feedback off", "Privacy", "Disables feedback prompts.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, -1),
        D("reg.tips-off", "Tips off", "Privacy", "Disables tips popups.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, 1),
        D("reg.lockscreen-tips", "Lock screen tips off", "Privacy", "Disables lock screen tips.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, 1),
        D("reg.defender-sample", "Defender auto-sample off", "Privacy", "Disables auto sample submission.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet", "SubmitSamplesConsent", 2, 1),
        D("reg.diag-off", "Diagnostic data off", "Privacy", "Disables diagnostic collection.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack", "ShowedToastAtLevel", 1, 0),
        D("reg.ink-off", "Ink Workspace off", "Privacy", "Disables Windows Ink.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsInkWorkspace", "AllowWindowsInkWorkspace", 0, 1),
        D("reg.handwriting-off", "Handwriting telemetry off", "Privacy", "Disables handwriting data sharing.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, 0),
        D("reg.cdp-off", "Connected Devices off", "Privacy", "Disables Connected Devices Platform.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableCdp", 0, 1),
        D("reg.tailored-off", "Tailored experiences off", "Privacy", "Disables personalized ads.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0, 1),
        D("reg.app-launch-track", "App launch tracking off", "Privacy", "Stops tracking launched apps.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, 1),
        D("reg.speech-update", "Speech data update off", "Privacy", "Disables speech model updates.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", 0, 1),
        D("reg.experiment-off", "Experimentation off", "Privacy", "Disables Windows feature experiments.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", 0, 1),
        D("reg.lockscreen-cam", "Lockscreen camera off", "Privacy", "Disables lock screen camera.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", 1, 0),
        D("reg.speech-online", "Online speech off", "Privacy", "Disables cloud speech recognition.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 0, 1),
        D("reg.settings-sync", "Settings sync off", "Privacy", "Stops syncing settings across devices.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SettingSync", "SyncPolicy", 5, 1),
        D("reg.lang-list", "Language list access off", "Privacy", "Prevents websites seeing language list.", _ => true, RegistryHive.CurrentUser, @"Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, 0),
        D("reg.user-activities", "User activities upload off", "Privacy", "Stops uploading activity history.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, 1),

        // ── AI Removal ──
        D("reg.copilot-off", "Copilot off", "AI Removal", "Disables Windows Copilot (user).", _ => true, RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0),
        D("reg.copilot-policy", "Copilot policy off", "AI Removal", "Disables Copilot (machine policy).", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0),
        D("reg.recall-off", "Recall off", "AI Removal", "Disables Windows Recall AI.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, 0),
        D("reg.recall-user", "Recall snapshots off", "AI Removal", "Disables Recall for user.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DisableAIDataAnalysis", 1, 0),
        D("reg.ai-companion", "AI companion off", "AI Removal", "Disables AI companion.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowAICompanion", 0, 1),
        D("reg.search-ai", "Search AI off", "AI Removal", "Disables cloud search AI.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsAADCloudSearchEnabled", 0, 1),
        D("reg.search-ai2", "Search highlights off", "AI Removal", "Disables dynamic search box.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0, 1),
        D("reg.input-insights", "Input insights off", "AI Removal", "Disables typing insights.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Input\Settings", "InsightsEnabled", 0, 1),
        D("reg.voice-access", "Voice access off", "AI Removal", "Disables voice activation.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Settings\VoiceActivation\UserPreferenceForAllApps", "AgentActivationEnabled", 0, 1),

        // ── Appearance (perf-first) ──
        D("reg.dark-apps", "Dark mode apps", "Appearance", "Apps use dark theme.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, 1),
        D("reg.dark-system", "Dark mode system", "Appearance", "System UI uses dark theme.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, 1),
        D("reg.transparency-off", "Transparency off", "Appearance", "Disables transparency to reduce GPU compositor overhead.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0, 1),
        D("reg.dwm-anim-off", "DWM animations off", "Appearance", "Disables DWM compositor animations system-wide.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DWM", "DisallowAnimations", 1, 0),
        D("reg.visual-fx", "Visual effects best perf", "Appearance", "Windows visual effects = best performance.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2, 0),
        D("reg.vbs-off", "VBS off (5-25% FPS gain)", "System", "Disables Virtualization-Based Security. Desktop only.", hw => hw.IsDesktop, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 0, 1),

        // ── Explorer ──
        D("reg.show-ext", "Show file extensions", "Explorer", "Shows file extensions.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, 1),
        D("reg.show-hidden", "Show hidden files", "Explorer", "Shows hidden files.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, 2),
        D("reg.open-thispc", "Open to This PC", "Explorer", "Explorer opens to This PC.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, 0),
        D("reg.no-explorer-ads", "No Explorer ads", "Explorer", "Disables sync provider notifications.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, 1),
        D("reg.widgets-off", "Widgets off", "Explorer", "Disables Widgets panel.", _ => true, RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, 1),
        D("reg.taskbar-search", "Taskbar search icon", "Explorer", "Search icon only on taskbar.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, 2),
        D("reg.chat-off", "Chat icon off", "Explorer", "Removes Chat from taskbar.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, 1),
        D("reg.low-disk-warn", "Low disk warning off", "Explorer", "Disables low disk space warnings.", _ => true, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoLowDiskSpaceChecks", 1, 0),
        new()
        {
            Id = "reg.sticky-keys", Name = "Sticky keys off", Category = "Explorer",
            Description = "Disables Sticky Keys shortcut.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadString(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", out var f) && f == "506",
            Apply = (p, _) => { p.Report("Sticky keys off"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "506"); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "510"); return Task.CompletedTask; }
        },
        new()
        {
            Id = "reg.old-ctx-menu", Name = "Classic context menu", Category = "Explorer",
            Description = "Restores the classic right-click menu in Windows 11.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.DefaultStringIs(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", ""),
            Apply = (p, _) => { p.Report("Classic context menu"); RegistryTweakHelper.WriteDefaultString(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", ""); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", ""); return Task.CompletedTask; }
        },

        // ── Visual ──
        D("reg.no-anim", "Window animations off", "Visual", "Disables minimize/maximize animations.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate", 0, 1),
        D("reg.drag-height", "Drag detection reduce", "Visual", "Reduces mouse drag threshold.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "DragHeight", 2, 4),
        D("reg.drag-width", "Drag detection width", "Visual", "Reduces horizontal drag threshold.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "DragWidth", 2, 4),
        D("reg.smooth-scroll", "Smooth scrolling off", "Visual", "Disables smooth scrolling.", _ => true, RegistryHive.CurrentUser, @"Control Panel\Desktop", "SmoothScroll", 0, 1),
        new()
        {
            Id = "reg.warn-sounds", Name = "Warning sounds off", Category = "Visual",
            Description = "Disables system beep sounds.",
            ShouldApply = _ => true,
            TryGetAppliedState = _ => RegistryTweakHelper.TryReadString(RegistryHive.CurrentUser, @"Control Panel\Sound", "Beep", out var b) && b == "no",
            Apply = (p, _) => { p.Report("Warning sounds off"); RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Sound", "Beep", "no"); return Task.CompletedTask; },
            Revert = (p, _) => { RegistryTweakHelper.WriteString(RegistryHive.CurrentUser, @"Control Panel\Sound", "Beep", "yes"); return Task.CompletedTask; }
        },

        // ── Storage ──
        D("reg.prefetch-off", "Prefetch off (SSD)", "Storage", "Disables Prefetch on SSD.", hw => hw.HasSsdBootVolume, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 0, 3),
        D("reg.superfetch-off", "Superfetch off (SSD)", "Storage", "Disables Superfetch on SSD.", hw => hw.HasSsdBootVolume, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 0, 3),
        D("reg.ntfs-lastaccess", "NTFS last access off", "Storage", "Disables last access timestamps.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate", 1, 0),
        D("reg.ntfs-8dot3", "NTFS 8.3 off", "Storage", "Disables legacy short filenames.", _ => true, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisable8dot3NameCreation", 1, 0),
        D("reg.ntfs-memuse", "NTFS memory high", "Storage", "Increases NTFS paged pool.", hw => hw.HasHighRam, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsMemoryUsage", 2, 1),
        D("reg.storage-idle", "Storage idle off", "Storage", "Disables storage D3 for consistent SSD perf.", hw => hw.IsDesktop, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Storage", "StorageD3InModernStandby", 0, 1),
    ];

    private static TweakOperation D(string id, string name, string cat, string desc, Func<HardwareInfo, bool> cond, RegistryHive hive, string key, string val, int apply, int revert) => new()
    {
        Id = id, Name = name, Category = cat, Description = desc, ShouldApply = cond,
        TryGetAppliedState = _ => RegistryTweakHelper.TryReadDword(hive, key, val, out var cur) && cur == apply,
        Apply = (p, _) => { p.Report(name); RegistryTweakHelper.WriteDword(hive, key, val, apply); return Task.CompletedTask; },
        Revert = (p, _) => { p.Report($"Revert {name}"); if (revert == -1) RegistryTweakHelper.DeleteValue(hive, key, val); else RegistryTweakHelper.WriteDword(hive, key, val, revert); return Task.CompletedTask; }
    };
}

internal static class HwRef { public static HardwareInfo? Hw { get; set; } }

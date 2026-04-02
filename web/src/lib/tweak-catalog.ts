import type { TweakItem } from "./types";

export const tweakCatalog: TweakItem[] = [
  // ── Input ──
  { id: "reg.mouse-queue", name: "Mouse queue 32", category: "Input", description: "Reduces mouse input buffer to 32 entries for lower latency", status: "unknown" },
  { id: "reg.kb-queue", name: "Keyboard queue 24", category: "Input", description: "Reduces keyboard input buffer to 24 entries for faster key response", status: "unknown" },
  { id: "reg.mouse-nolazy", name: "Mouse no lazy mode", category: "Input", description: "Disables lazy input processing for mouse events", status: "unknown" },
  { id: "reg.kb-nolazy", name: "Keyboard no lazy mode", category: "Input", description: "Disables lazy input processing for keyboard events", status: "unknown" },
  { id: "reg.mouse-freq", name: "Mouse sample rate max", category: "Input", description: "Maximizes mouse polling sample rate to 250 Hz", status: "unknown" },

  // ── System ──
  { id: "reg.sys-resp", name: "SystemResponsiveness 0", category: "System", description: "Sets multimedia system responsiveness to 0 for maximum foreground priority", status: "unknown" },
  { id: "reg.net-throttle", name: "Disable network throttle", category: "System", description: "Removes network throttling index limit for multimedia streaming", status: "unknown" },
  { id: "reg.priority-sep", name: "Win32PrioritySeparation", category: "System", description: "Optimizes CPU quantum scheduling — AMD (0x28) or Intel (0x26)", status: "unknown" },
  { id: "reg.fg-lock", name: "ForegroundLockTimeout 0", category: "System", description: "Instantly switches focus to foreground applications", status: "unknown" },
  { id: "reg.hooks-timeout", name: "LowLevelHooksTimeout 0", category: "System", description: "Eliminates delay for low-level keyboard/mouse hooks", status: "unknown" },
  { id: "reg.menu-delay", name: "MenuShowDelay 0", category: "System", description: "Removes delay before menus appear", status: "unknown" },
  { id: "reg.kill-svc", name: "WaitToKillService 2s", category: "System", description: "Reduces service shutdown timeout from 5s to 2s for faster restarts", status: "unknown" },
  { id: "reg.hung-app", name: "HungAppTimeout 2s", category: "System", description: "Faster detection and closure of unresponsive applications", status: "unknown" },
  { id: "reg.boot-delay", name: "Disable startup delay", category: "System", description: "Removes artificial delay before startup programs launch", status: "unknown" },
  { id: "reg.bg-apps-off", name: "Background apps off", category: "System", description: "Prevents UWP apps from running in the background", status: "unknown" },
  { id: "reg.mmcss-gaming", name: "MMCSS gaming priority", category: "System", description: "Elevates thread priority for processes in the Games MMCSS class", status: "unknown" },
  { id: "reg.mmcss-sched", name: "MMCSS scheduling Games", category: "System", description: "Sets Games task scheduling category to High for the multimedia class scheduler", status: "unknown" },
  { id: "reg.mmcss-sfio", name: "MMCSS SFIO priority", category: "System", description: "Raises scheduled file I/O priority for game processes", status: "unknown" },

  // ── Memory ──
  { id: "reg.large-cache", name: "LargeSystemCache", category: "Memory", description: "Enables large system cache for improved file caching", conditional: "≥ 32 GB RAM", status: "unknown" },
  { id: "reg.no-paging-exec", name: "DisablePagingExecutive", category: "Memory", description: "Keeps kernel code in RAM instead of paging to disk", conditional: "≥ 32 GB RAM", status: "unknown" },
  { id: "reg.nonpaged-pool", name: "NonPagedPoolSize max", category: "Memory", description: "Maximizes non-paged memory pool for device drivers", conditional: "≥ 32 GB RAM", status: "unknown" },
  { id: "svc.memcompress", name: "Memory compression off", category: "Memory", description: "Disables memory compression — uses raw RAM instead of CPU cycles", conditional: "≥ 32 GB RAM", status: "unknown" },

  // ── Gaming ──
  { id: "reg.gamemode-off", name: "Game Mode off", category: "Gaming", description: "Disables Windows Game Mode which can cause frame inconsistency", status: "unknown" },
  { id: "reg.gamemode-off2", name: "AllowAutoGameMode off", category: "Gaming", description: "Prevents Windows from automatically enabling Game Mode", status: "unknown" },
  { id: "reg.gamedvr-off", name: "GameDVR off", category: "Gaming", description: "Disables background game recording and Xbox Game Bar capture", status: "unknown" },
  { id: "reg.appcapture", name: "AppCapture off", category: "Gaming", description: "Blocks app capture policy to fully disable game recording", status: "unknown" },
  { id: "reg.fso-global", name: "Fullscreen optimizations off", category: "Gaming", description: "Disables FSO globally — forces true exclusive fullscreen in games", status: "unknown" },
  { id: "reg.fso-dsc", name: "Fullscreen DSC compat off", category: "Gaming", description: "Disables DXGI FSE compatibility layer for purer rendering", status: "unknown" },
  { id: "reg.fso-hgm", name: "Fullscreen HGM off", category: "Gaming", description: "Honors user fullscreen behavior mode for consistent game rendering", status: "unknown" },
  { id: "reg.gpu-perf-pref", name: "GPU high performance preference", category: "Gaming", description: "Sets global DirectX GPU preference to high performance", status: "unknown" },

  // ── Privacy ──
  { id: "reg.telemetry", name: "AllowTelemetry 0", category: "Privacy", description: "Disables Windows telemetry data collection", status: "unknown" },
  { id: "reg.toast-off", name: "Toast notifications off", category: "Privacy", description: "Disables push notification toasts", status: "unknown" },
  { id: "reg.notif-off", name: "Notification Center off", category: "Privacy", description: "Hides the Action/Notification Center", status: "unknown" },
  { id: "reg.adid-off", name: "Ad ID off", category: "Privacy", description: "Disables advertising identifier for cross-app tracking", status: "unknown" },
  { id: "reg.activity-off", name: "Activity feed off", category: "Privacy", description: "Disables Windows activity history and Timeline", status: "unknown" },
  { id: "reg.cortana-off", name: "Cortana off", category: "Privacy", description: "Disables Cortana assistant entirely", status: "unknown" },
  { id: "reg.bing-off", name: "Bing search off", category: "Privacy", description: "Removes Bing web results from Start Menu search", status: "unknown" },
  { id: "reg.content-off", name: "Content delivery off", category: "Privacy", description: "Stops Microsoft content delivery (suggested apps, tips)", status: "unknown" },
  { id: "reg.location-off", name: "Location tracking off", category: "Privacy", description: "Disables location services system-wide", status: "unknown" },
  { id: "reg.feedback-off", name: "Feedback notifications off", category: "Privacy", description: "Stops Windows feedback prompts and SIUF surveys", status: "unknown" },
  { id: "reg.tips-off", name: "Tips and suggestions off", category: "Privacy", description: "Disables tips, tricks, and suggestion notifications", status: "unknown" },
  { id: "reg.lockscreen-tips", name: "Lock screen tips off", category: "Privacy", description: "Removes rotating tips and ads from the lock screen", status: "unknown" },
  { id: "reg.defender-sample", name: "Defender auto-sample off", category: "Privacy", description: "Prevents Defender from automatically submitting file samples", status: "unknown" },
  { id: "reg.diag-off", name: "Diagnostic data off", category: "Privacy", description: "Reduces diagnostic and usage data collection", status: "unknown" },
  { id: "reg.ink-off", name: "Windows Ink Workspace off", category: "Privacy", description: "Disables Windows Ink Workspace overlay", status: "unknown" },

  // ── Explorer / Visual ──
  { id: "reg.show-ext", name: "Show file extensions", category: "Explorer", description: "Displays file extensions for all file types in Explorer", status: "unknown" },
  { id: "reg.show-hidden", name: "Show hidden files", category: "Explorer", description: "Shows hidden files and folders in Explorer", status: "unknown" },
  { id: "reg.open-thispc", name: "Open to This PC", category: "Explorer", description: "Explorer opens to This PC instead of Quick Access", status: "unknown" },
  { id: "reg.no-explorer-ads", name: "No Explorer ads", category: "Explorer", description: "Disables sync provider ads and promotions in Explorer", status: "unknown" },
  { id: "reg.widgets-off", name: "Widgets off", category: "Explorer", description: "Disables Windows 11 widgets panel and news feed", status: "unknown" },
  { id: "reg.no-anim", name: "Window animations off", category: "Visual", description: "Disables minimize/maximize window animations", status: "unknown" },
  { id: "reg.smooth-scroll", name: "Smooth scrolling off", category: "Visual", description: "Disables smooth scrolling for snappier scroll response", status: "unknown" },

  // ── Storage / NTFS ──
  { id: "reg.prefetch-off", name: "Prefetch off (SSD)", category: "Storage", description: "Disables prefetch on SSD boot volume — unnecessary with fast storage", conditional: "SSD boot drive", status: "unknown" },
  { id: "reg.superfetch-off", name: "Superfetch off (SSD)", category: "Storage", description: "Disables SysMain/Superfetch on SSD — reduces write wear", conditional: "SSD boot drive", status: "unknown" },
  { id: "reg.ntfs-lastaccess", name: "NTFS last access off", category: "Storage", description: "Disables updating last access timestamps — reduces I/O overhead", status: "unknown" },
  { id: "reg.ntfs-8dot3", name: "NTFS 8.3 naming off", category: "Storage", description: "Disables legacy 8.3 short filename creation for faster directory ops", status: "unknown" },
  { id: "reg.ntfs-memuse", name: "NTFS memory usage high", category: "Storage", description: "Increases NTFS paged pool memory allocation for better file performance", conditional: "≥ 32 GB RAM", status: "unknown" },

  // ── Services ──
  { id: "svc.wsearch", name: "Disable Windows Search", category: "Services", description: "Stops background indexing — reduces disk and CPU usage", status: "unknown" },
  { id: "svc.sysmain", name: "Disable SysMain", category: "Services", description: "Stops Superfetch service from pre-loading apps into memory", status: "unknown" },
  { id: "svc.dosvc", name: "Disable Delivery Optimization", category: "Services", description: "Stops peer-to-peer Windows Update sharing", status: "unknown" },
  { id: "svc.spooler", name: "Disable Print Spooler", category: "Services", description: "Stops print spooler service if no printer is used", status: "unknown" },
  { id: "svc.fax", name: "Disable Fax", category: "Services", description: "Stops the legacy Fax service", status: "unknown" },
  { id: "svc.diagtrack", name: "Disable DiagTrack", category: "Services", description: "Stops Connected User Experiences and Telemetry service", status: "unknown" },
  { id: "svc.xblauthmanager", name: "Disable Xbox Auth", category: "Services", description: "Stops Xbox Live authentication manager service", status: "unknown" },
  { id: "svc.xblgamesave", name: "Disable Xbox GameSave", category: "Services", description: "Stops Xbox Live game save sync service", status: "unknown" },
  { id: "svc.xboxnetapisvc", name: "Disable Xbox Net API", category: "Services", description: "Stops Xbox Live networking service", status: "unknown" },
  { id: "svc.wersvc", name: "Disable Error Reporting", category: "Services", description: "Stops Windows Error Reporting service", status: "unknown" },
  { id: "svc.mapsbroker", name: "Disable Maps Broker", category: "Services", description: "Stops downloaded maps manager service", status: "unknown" },
  { id: "svc.wpnservice", name: "Disable Push Notifications", category: "Services", description: "Stops Windows Push Notification service", status: "unknown" },
  { id: "svc.remoteregistry", name: "Disable Remote Registry", category: "Services", description: "Stops remote registry access for better security", status: "unknown" },
  { id: "svc.dmwappushservice", name: "Disable WAP Push", category: "Services", description: "Stops device management WAP push message routing", status: "unknown" },
  { id: "svc.bits", name: "Disable BITS", category: "Services", description: "Stops Background Intelligent Transfer Service when not needed", status: "unknown" },
  { id: "svc.bth-manual", name: "Bluetooth to Manual", category: "Services", description: "Sets Bluetooth service to manual start instead of automatic", status: "unknown" },
  { id: "svc.lfsvc", name: "Disable Geolocation", category: "Services", description: "Stops the Windows Geolocation service", status: "unknown" },

  // ── Power ──
  { id: "pwr.usb-wmi", name: "USB WMI power off", category: "Power", description: "Disables WMI power management on all USB devices", conditional: "Desktop or 5+ USB", status: "unknown" },
  { id: "pwr.usb-suspend", name: "USB selective suspend off", category: "Power", description: "Prevents USB devices from entering low-power suspend mode", conditional: "Desktop", status: "unknown" },
  { id: "pwr.pcie-off", name: "PCIe ASPM off", category: "Power", description: "Disables PCIe Active State Power Management for max throughput", conditional: "Desktop", status: "unknown" },
  { id: "pwr.hibernate-off", name: "Hibernate off", category: "Power", description: "Disables hibernation — frees disk space equal to RAM size", conditional: "Desktop", status: "unknown" },
  { id: "pwr.ultimate", name: "Ultimate Performance plan", category: "Power", description: "Creates and activates Ultimate Performance power plan", status: "unknown" },
  { id: "pwr.core-unpark", name: "Unpark CPU cores", category: "Power", description: "Keeps all CPU cores active at 100% minimum processor state", conditional: "Desktop", status: "unknown" },
  { id: "pwr.throttle-off", name: "Power throttling off", category: "Power", description: "Disables power throttling for sustained max CPU performance", conditional: "Desktop", status: "unknown" },
  { id: "pwr.hdd-timeout", name: "Disk never sleep", category: "Power", description: "Prevents hard disks from entering sleep mode", conditional: "Desktop", status: "unknown" },
  { id: "pwr.cpu-max", name: "CPU max performance 100%", category: "Power", description: "Sets maximum processor state to 100% under all conditions", status: "unknown" },

  // ── GPU ──
  { id: "gpu.mpo-off", name: "MPO off (multi-monitor)", category: "GPU", description: "Disables Multi-Plane Overlay to fix stuttering on multi-monitor setups", conditional: "2+ monitors", status: "unknown" },
  { id: "gpu.hags", name: "HAGS config", category: "GPU", description: "Configures Hardware Accelerated GPU Scheduling based on VRAM", status: "unknown" },
  { id: "gpu.tdr-delay", name: "TDR delay 10s", category: "GPU", description: "Increases GPU timeout detection delay to prevent false resets", conditional: "2+ monitors", status: "unknown" },
  { id: "gpu.nv-shader", name: "NVIDIA shader cache", category: "GPU", description: "Enables NVIDIA shader pre-cache for faster game loading", conditional: "NVIDIA GPU", status: "unknown" },
  { id: "gpu.nv-pstate", name: "NVIDIA P-State 0", category: "GPU", description: "Locks NVIDIA GPU to max performance P-State", conditional: "NVIDIA GPU", status: "unknown" },
  { id: "gpu.dwm-vblank", name: "DWM VBlank optimized", category: "GPU", description: "Disables flip wait override in Desktop Window Manager", status: "unknown" },
  { id: "gpu.game-sched", name: "GPU game scheduling", category: "GPU", description: "Sets GPU scheduling priority to 8 for game processes", status: "unknown" },

  // ── Network ──
  { id: "net.tcp", name: "TCP optimization", category: "Network", description: "Disables heuristics, enables RSS, disables timestamps and ECN", status: "unknown" },
  { id: "net.nagle", name: "Nagle off", category: "Network", description: "Disables Nagle's algorithm on all TCP interfaces for lower latency", status: "unknown" },
  { id: "net.lso-off", name: "Large Send Offload off", category: "Network", description: "Disables LSO to prevent batched packet sending delays", status: "unknown" },
  { id: "net.nic-power", name: "NIC power management off", category: "Network", description: "Prevents network adapters from entering power-saving mode", conditional: "Desktop", status: "unknown" },
  { id: "net.dns-cache", name: "DNS cache optimized", category: "Network", description: "Extends positive DNS cache TTL and shortens negative cache", status: "unknown" },

  // ── Cleanup ──
  { id: "clean.temp", name: "Clean TEMP", category: "Cleanup", description: "Removes temporary files from user and system TEMP directories", status: "unknown" },
  { id: "clean.prefetch", name: "Clean Prefetch", category: "Cleanup", description: "Clears Windows Prefetch cache files", status: "unknown" },
  { id: "clean.softdist", name: "Clean WU cache", category: "Cleanup", description: "Clears Windows Update download cache", status: "unknown" },
  { id: "clean.events", name: "Clear event logs", category: "Cleanup", description: "Clears all Windows event logs to free disk space", status: "unknown" },
  { id: "clean.dism", name: "DISM cleanup", category: "Cleanup", description: "Runs DISM component cleanup to remove superseded update packages", status: "unknown" },
];

export const categoryOrder: string[] = [
  "Input",
  "System",
  "Memory",
  "Gaming",
  "Privacy",
  "Explorer",
  "Visual",
  "Storage",
  "Services",
  "Power",
  "GPU",
  "Network",
  "Cleanup",
];

export const categoryIcons: Record<string, string> = {
  Input: "mouse-pointer-2",
  System: "cpu",
  Memory: "memory-stick",
  Gaming: "gamepad-2",
  Privacy: "shield-check",
  Explorer: "folder-open",
  Visual: "eye",
  Storage: "hard-drive",
  Services: "server",
  Power: "zap",
  GPU: "monitor",
  Network: "wifi",
  Cleanup: "trash-2",
};

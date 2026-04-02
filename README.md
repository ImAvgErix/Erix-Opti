# ErixOpti

**The ultimate safe, beautiful, and intelligent gaming optimization companion for Windows 11.**

ErixOpti is a WinUI 3 desktop application for **safe, reversible** Windows 11 tuning aimed at gaming and responsiveness. It combines live WMI + registry + BCD awareness, automatic backups before changes, hardware-aware tweak logic, Serilog diagnostics, and a modern MVVM-driven UI with glassmorphism design.

## Features

- **Hardware Snapshot** — Live CPU, GPU, RAM, storage, motherboard, network, and OS data refreshed every 5 seconds via WMI, registry, and performance counters
- **15 Optimizations** — Grouped into Performance & Gaming, Services & Background, Privacy & Telemetry, Visual & System, Storage & Memory, and Advanced / High Risk
- **Hardware-aware** — Desktop vs laptop detection, GPU vendor-specific recommendations, SSD-only tweaks, battery/AC awareness
- **Presets** — One-click Gaming, Privacy, Balanced, and Extreme profiles
- **Automatic Backup** — System Restore point + full registry export + BCD export before any tweak
- **Double Confirmation** — High-risk tweaks require two-step user consent
- **Auto-revert on Failure** — If an apply fails, automatic rollback is attempted
- **Quick Tools** — One-click runtime downloads, vendor driver links, external utility launchers, Control Panel shortcuts, restore point creation, and full hardware report export
- **Diagnostics Log** — Real-time Serilog output in-app with export to desktop

## Requirements

- Windows 11 (build **10.0.26100** or newer recommended)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Administrator elevation (manifest + self-restart)

## Build and run

```powershell
dotnet restore ErixOpti.sln
dotnet build ErixOpti.sln -c Debug -r win-x64
dotnet run --project ErixOpti/ErixOpti.csproj -r win-x64
```

## Publish single-file .exe

```powershell
dotnet publish ErixOpti/ErixOpti.csproj -c Release -r win-x64 `
  -p:PublishSingleFile=true `
  -p:SelfContained=true `
  -p:WindowsAppSDKSelfContained=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

## Tech stack (pinned April 2026)

| Component | Version |
| --- | --- |
| Target framework | `net10.0-windows10.0.26100.0` |
| Windows App SDK | `1.8.260317003` |
| Microsoft.Windows.SDK.BuildTools | `10.0.28000.1721` |
| CommunityToolkit.Mvvm | `8.4.2` |
| CommunityToolkit WinUI (Settings + Animations) | `8.2.251219` |
| Microsoft.Extensions.Hosting | `10.0.0` |
| Serilog + Serilog.Sinks.File | `4.3.1` / `6.0.0` |
| Serilog.Extensions.Logging | `9.0.0` |
| System.Management | `10.0.0` |

## Project structure

```
Erix-Opti/
├── ErixOpti.sln
├── ErixOpti.Core/           # Platform-independent logic
│   ├── Models/              # HardwareInfo, enums
│   ├── Interfaces/          # ITweak, IHardwareService, IBackupService, etc.
│   ├── Helpers/             # AdminHelper, ProcessRunner, RegistryTweakHelper
│   ├── Services/            # HardwareService, BackupService, TweakCatalog
│   │   └── Tweaks/          # 15 ITweak implementations
│   └── ViewModels/          # Hardware, Optimizations, QuickTools, Log VMs
├── ErixOpti/                # WinUI 3 UI project
│   ├── App.xaml(.cs)        # DI host, Serilog, admin gate
│   ├── MainWindow.xaml(.cs) # NavigationView shell
│   ├── Views/               # Hardware, Optimizations, QuickTools, Log pages
│   ├── Services/            # UiLogSink, UserDialogService, WindowContext
│   └── Resources/           # Theme.xaml (dark palette, glassmorphism)
├── .github/workflows/       # CI/CD: build, publish, GitHub Release
└── README.md
```

## Optimizations included

| Category | Tweak | Risk |
| --- | --- | --- |
| Performance & Gaming | HAGS, Game Mode, Ultimate Performance plan, ReBAR hints, SystemResponsiveness, Core Parking | Low–Medium |
| Services & Background | Disable DiagTrack, dmwappushservice, WSearch | Medium |
| Privacy & Telemetry | Telemetry policy, Advertising ID, Activity History, Cortana | Low |
| Visual & System | Reduce animations/transparency/visual effects | Low |
| Storage & Memory | Aggressive Storage Sense, Disable Prefetch/Superfetch (SSD only) | Low–Medium |
| Advanced / High Risk | Disable HVCI, BCD boot UX, Spectre mitigations | High |

## Safety

- First tweak apply in a session prompts for **System Restore point + registry + BCD export**
- High-risk tweaks require **two-step confirmation** dialogs
- Apply failures attempt **automatic revert** for the same tweak
- All operations fully async with `IProgress<string>` + `CancellationToken`
- Never touches critical services without explicit user consent

## License

Proprietary — all rights reserved unless a license file is added.

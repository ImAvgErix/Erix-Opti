# ErixOpti

One-click, hardware-aware Windows 11 optimization. No toggles, no guesswork — ErixOpti reads your hardware, builds a plan, and applies 100+ tweaks in a single button press.

## Features

### Auto Optimize
Press the button. ErixOpti creates a system restore point and registry backup, then applies every applicable tweak in order. Desktop and laptop paths differ automatically.

### Hardware Detection
Live WMI snapshot of CPU, GPU, RAM (installed DIMM capacity + speed + type), storage, motherboard, BIOS, network, monitors, and USB devices. Detects AMD vs Intel, NVIDIA vs AMD GPU, SSD boot, multi-monitor, and form factor to tailor the optimization plan.

### Tweak Catalog

| Category | Count | Examples |
|---|---|---|
| **Input** | 5 | Mouse/keyboard queue depth, no-lazy mode, sample rate |
| **System** | 15+ | SystemResponsiveness, PrioritySeparation (AMD/Intel), MMCSS gaming, SvcHostSplit, startup delay |
| **Memory** | 4 | LargeSystemCache, DisablePagingExecutive, IoPageLockLimit (high-RAM only) |
| **Gaming** | 9 | Game Mode off, GameDVR off, FSO off, Game Bar off, GPU preference |
| **GPU** | 7 | MPO off, HAGS (VRAM-aware), TDR delay, NVIDIA P-State 0, DWM VBlank, GPU scheduling |
| **Power** | 10 | **Erix Gaming** custom power plan (desktop/laptop variants), timer resolution, USB/PCIe power, hibernate, CPU unpark, throttling off |
| **Network** | 5 | TCP stack, Nagle off, LSO off, NIC power, DNS cache |
| **Privacy** | 18 | Telemetry, Cortana, Bing search, ad ID, location, feedback, Defender auto-sample, diagnostic data |
| **AI Removal** | 7 | Copilot off (user + policy), Recall off, AI companion, search AI/highlights |
| **Dark Mode** | 3 | Apps + system dark theme, transparency on |
| **Explorer** | 7 | File extensions, hidden files, This PC, no ads, widgets off, search icon, chat off |
| **Visual** | 4 | Window animations off, drag detection, smooth scroll off |
| **Storage** | 5 | Prefetch/Superfetch off (SSD), NTFS last-access off, 8.3 naming off |
| **Services** | 39 | WSearch, SysMain, DiagTrack, Xbox services, WaaSMedicSvc, and more |
| **Cleanup** | 5 | TEMP, Prefetch, Windows Update cache, event logs, DISM component cleanup |

### Custom Power Plan
Auto Optimize creates an **Erix Gaming** power plan based on Ultimate Performance. Desktop gets everything maxed (no sleep, no timeouts, cores unparked). Laptop gets battery-aware DC fallbacks while keeping AC performance at max.

### Status Detection
Every registry and service tweak has a live status probe. Power tweaks read the active scheme from the registry. Network tweaks run async probes. The catalog shows **Active**, **Pending**, **One-shot**, or **Skipped** for each tweak in real time.

## Requirements

- Windows 11 (build 22000+)
- .NET 10 runtime (bundled in single-file release)
- Administrator privileges (auto-elevates on launch)

## Installation

Download `ErixOpti.exe` from the [latest release](../../releases/latest) and run it. No installer needed — it's a single self-contained executable.

## Building from Source

```
git clone https://github.com/YOUR_USER/Erix-Opti.git
cd Erix-Opti
dotnet build -c Release
dotnet publish ErixOpti/ErixOpti.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
```

## Versioning

Releases follow `v1.0`, `v1.1`, `v1.2`, etc. Tag a commit to trigger a release build.

## License

All rights reserved. This software is provided as-is for personal use.

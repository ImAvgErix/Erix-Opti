# ErixOpti v2

The ultimate safe, intelligent gaming optimization companion for Windows 11.

## Features

### Hardware Dashboard
- Static WMI-powered system snapshot: CPU, GPU, RAM (type/speed/slots), storage, motherboard, network, peripherals
- Monitor count, USB device count, audio device detection
- One-click hardware report export

### Auto Optimize (One-Click)
- Single button applies 60+ hardware-conditional tweaks automatically
- Automatic backup (System Restore + Registry + BCD export) before any changes
- Hardware-aware logic:
  - AMD CPU → Win32PrioritySeparation 0x28 | Intel → 0x26
  - RAM >= 32 GB → LargeSystemCache, DisablePagingExecutive, NonPagedPoolSize
  - Multi-monitor → MPO off, TDR delay increase
  - VRAM <= 8 GB → HAGS off | > 8 GB → HAGS on
  - NVIDIA → Shader pre-cache | Desktop → USB/PCIe power off, hibernate off
  - SSD boot → Prefetch/Superfetch off

### Tweak Categories
- **Registry** (30+): Input, system responsiveness, memory, gaming, privacy, explorer
- **Services** (31): Full disable list from WSearch to WaaSMedicSvc
- **Power** (6): USB WMI, selective suspend, PCIe ASPM, hibernate, Ultimate Performance, core unpark
- **GPU** (4): MPO, HAGS, TDR, NVIDIA shader cache
- **Network** (2): TCP optimization, Nagle disable
- **Cleanup** (5): TEMP, Prefetch, WU cache, event logs, DISM

### Downloads
- In-app download manager with progress bars
- VC++ Redist, DirectX, .NET runtimes (direct download + silent install)
- GPU/chipset driver links (NVIDIA, AMD, Intel)
- Utility links (GPU-Z, HWiNFO, DDU, NVCleanInstall, NPI)

### Tools
- Deep system cleanup with progress tracking
- System restore point creation
- Control panel shortcuts (Programs, Power, Device Manager, Services, Disk Management, Event Viewer, Firewall)

## Tech Stack
- WinUI 3 / Windows App SDK 1.8
- .NET 10, CommunityToolkit.Mvvm, Serilog
- Mica backdrop, dark theme (#09090B), cyan accent (#00B4FF)

## Build
```powershell
dotnet build ErixOpti.sln -c Release
```

## Requirements
- Windows 11 (build 10.0.26100+)
- Administrator elevation (auto-prompts)
- .NET 10 SDK

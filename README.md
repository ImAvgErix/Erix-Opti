# ErixOpti v1

Hardware-aware Windows 11 tuning with one-click **Auto Optimize** (registry, services, power, GPU, network, cleanup). Implemented in this repo; runs elevated with automatic restore-point + registry export before changes.

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

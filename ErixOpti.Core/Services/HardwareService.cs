using System.Management;
using System.Runtime.InteropServices;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services;

public sealed class HardwareService : IHardwareService, IDisposable
{
    private readonly HardwareInfo _current = new();
    public HardwareInfo Current => _current;
    public event EventHandler? HardwareUpdated;

    public async Task StartAsync(CancellationToken ct = default)
    {
        await Task.Run(() => DetectAll(ct), ct).ConfigureAwait(false);
        HardwareUpdated?.Invoke(this, EventArgs.Empty);
    }

    public Task RefreshAsync(CancellationToken ct = default) => StartAsync(ct);

    public Task StopAsync() => Task.CompletedTask;
    public void Dispose() { }

    private void DetectAll(CancellationToken ct)
    {
        DetectFormFactor(ct); DetectPower(); DetectCpu(ct); DetectGpu(ct);
        DetectRam(ct); DetectStorage(ct); DetectMotherboard(ct); DetectNetwork(ct);
        DetectOs(); DetectMonitors(ct); DetectUsb(ct); DetectAudio(ct);
        _current.Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
    }

    private void DetectFormFactor(CancellationToken ct)
    {
        var laptopSet = new HashSet<ushort>{8,9,10,14,18,21};
        var desktopSet = new HashSet<ushort>{3,4,5,6,7,13,15,16,35,36};
        var factor = FormFactor.Unknown;
        using (var s = new ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure"))
        {
            foreach (ManagementObject mo in s.Get())
            {
                ct.ThrowIfCancellationRequested();
                if (mo["ChassisTypes"] is ushort[] arr)
                {
                    foreach (var t in arr)
                    {
                        if (laptopSet.Contains(t)) { factor = FormFactor.Laptop; break; }
                        if (desktopSet.Contains(t) && factor == FormFactor.Unknown) factor = FormFactor.Desktop;
                    }
                }
                mo.Dispose();
                if (factor == FormFactor.Laptop) break;
            }
        }
        if (factor == FormFactor.Unknown) { using var b = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_Battery"); foreach(ManagementObject mo in b.Get()){factor=FormFactor.Laptop;mo.Dispose();break;} }
        _current.FormFactor = factor == FormFactor.Unknown ? FormFactor.Desktop : factor;
    }

    private void DetectPower()
    {
        try { var s = new SystemPowerStatus(); if (GetSystemPowerStatus(ref s)) _current.PowerSource = s.ACLineStatus == 1 ? PowerSource.Ac : s.ACLineStatus == 0 ? PowerSource.Battery : PowerSource.Unknown; } catch { }
    }

    private void DetectCpu(CancellationToken ct)
    {
        using var s = new ManagementObjectSearcher("SELECT Name,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed,Manufacturer,Architecture,L2CacheSize,L3CacheSize FROM Win32_Processor");
        foreach (ManagementObject mo in s.Get())
        {
            ct.ThrowIfCancellationRequested();
            _current.CpuName = (mo["Name"]?.ToString() ?? "—").Trim();
            _current.CpuCores = Convert.ToInt32(mo["NumberOfCores"] ?? 0);
            _current.CpuLogicalProcessors = Convert.ToInt32(mo["NumberOfLogicalProcessors"] ?? 0);
            _current.CpuMaxClockMhz = Convert.ToInt32(mo["MaxClockSpeed"] ?? 0);
            _current.CpuManufacturer = (mo["Manufacturer"]?.ToString() ?? "—").Trim();
            _current.CpuL2CacheKb = Convert.ToInt32(mo["L2CacheSize"] ?? 0);
            _current.CpuL3CacheKb = Convert.ToInt32(mo["L3CacheSize"] ?? 0);
            var arch = Convert.ToInt32(mo["Architecture"] ?? 0);
            _current.CpuArchitecture = arch switch { 9 => "x64", 12 => "ARM64", 0 => "x86", _ => $"Arch{arch}" };
            mo.Dispose(); break;
        }
    }

    private void DetectGpu(CancellationToken ct)
    {
        using var s = new ManagementObjectSearcher("SELECT Name,AdapterRAM,DriverVersion,DriverDate FROM Win32_VideoController");
        foreach (ManagementObject mo in s.Get())
        {
            ct.ThrowIfCancellationRequested();
            var n = mo["Name"]?.ToString();
            if (string.IsNullOrWhiteSpace(n) || n!.Contains("Basic", StringComparison.OrdinalIgnoreCase)) { mo.Dispose(); continue; }
            _current.GpuName = n;
            var ram = Convert.ToUInt64(mo["AdapterRAM"] ?? 0UL);
            _current.GpuMemoryGb = ram / (1024.0 * 1024.0 * 1024.0);
            if (_current.GpuMemoryGb < 0.01) _current.GpuMemoryGb = 0;
            _current.GpuDriverVersion = mo["DriverVersion"]?.ToString() ?? "—";
            if (mo["DriverDate"] is string ds) try { _current.GpuDriverDate = ManagementDateTimeConverter.ToDateTime(ds).ToString("d"); } catch { }
            var u = n.ToUpperInvariant();
            _current.PrimaryGpuVendor = u.Contains("NVIDIA") ? GpuVendor.Nvidia : u.Contains("AMD") || u.Contains("RADEON") ? GpuVendor.Amd : u.Contains("INTEL") ? GpuVendor.Intel : GpuVendor.Unknown;
            mo.Dispose(); break;
        }

        // WMI AdapterRAM is uint32 — capped at 4 GB. Use registry for real VRAM.
        if (_current.GpuMemoryGb <= 4.0)
        {
            try
            {
                const string basePath = @"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
                using var baseKey = Registry.LocalMachine.OpenSubKey(basePath);
                if (baseKey != null)
                {
                    foreach (var sub in baseKey.GetSubKeyNames())
                    {
                        using var devKey = baseKey.OpenSubKey(sub);
                        var val = devKey?.GetValue("HardwareInformation.qwMemorySize");
                        if (val is long qw && qw > 0)
                        {
                            _current.GpuMemoryGb = qw / (1024.0 * 1024.0 * 1024.0);
                            break;
                        }
                    }
                }
            }
            catch { /* Registry VRAM fallback unavailable */ }
        }
    }

    private void DetectRam(CancellationToken ct)
    {
        using (var os = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem"))
            foreach (ManagementObject mo in os.Get()) { ct.ThrowIfCancellationRequested(); var tk = Convert.ToDouble(mo["TotalVisibleMemorySize"]??0); var fk = Convert.ToDouble(mo["FreePhysicalMemory"]??0); _current.RamTotalGb = tk/1048576.0; _current.RamAvailableGb = fk/1048576.0; mo.Dispose(); break; }
        int speed = 0, slots = 0;
        ulong installedBytes = 0;
        var typesSeen = new HashSet<string>();
        using (var m = new ManagementObjectSearcher("SELECT Speed,SMBIOSMemoryType,Capacity FROM Win32_PhysicalMemory"))
        {
            foreach (ManagementObject mo in m.Get())
            {
                ct.ThrowIfCancellationRequested();
                slots++;
                installedBytes += Convert.ToUInt64(mo["Capacity"] ?? 0UL);
                var sp = Convert.ToInt32(mo["Speed"] ?? 0);
                if (sp > speed) speed = sp;
                var t = Convert.ToInt32(mo["SMBIOSMemoryType"] ?? 0);
                typesSeen.Add(t switch { 26 => "DDR4", 34 => "DDR5", 24 => "DDR3", _ => $"Type{t}" });
                mo.Dispose();
            }
        }
        _current.RamInstalledGb = installedBytes / (1024.0 * 1024.0 * 1024.0);
        _current.RamSpeedMhz = speed;
        _current.RamType = typesSeen.Count == 0 ? "—" : string.Join(" / ", typesSeen);
        _current.RamSlotsUsed = slots;
    }

    private void DetectStorage(CancellationToken ct)
    {
        var parts = new List<string>(); bool hasSsd = false;
        using var s = new ManagementObjectSearcher("SELECT Model,Size,MediaType FROM Win32_DiskDrive");
        foreach (ManagementObject mo in s.Get())
        {
            ct.ThrowIfCancellationRequested();
            var model = mo["Model"]?.ToString() ?? "Disk"; var size = Convert.ToUInt64(mo["Size"]??0UL); var media = mo["MediaType"]?.ToString() ?? "";
            var gb = size / (1024.0*1024.0*1024.0);
            var kind = (media.Contains("SSD",StringComparison.OrdinalIgnoreCase)||model.Contains("SSD",StringComparison.OrdinalIgnoreCase)||model.Contains("NVMe",StringComparison.OrdinalIgnoreCase)) ? "SSD" : "HDD";
            if (kind=="SSD") hasSsd = true;
            parts.Add($"{model} ({gb:0.#} GB {kind})"); mo.Dispose();
        }
        _current.HasSsdBootVolume = hasSsd;
        _current.StorageSummary = parts.Count == 0 ? "—" : string.Join("; ", parts.Take(4));
    }

    private void DetectMotherboard(CancellationToken ct)
    {
        using (var s = new ManagementObjectSearcher("SELECT Manufacturer,Product FROM Win32_BaseBoard"))
            foreach (ManagementObject mo in s.Get()) { ct.ThrowIfCancellationRequested(); _current.Motherboard = $"{mo["Manufacturer"]} {mo["Product"]}".Trim(); mo.Dispose(); break; }
        using (var s = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion FROM Win32_BIOS"))
            foreach (ManagementObject mo in s.Get()) { ct.ThrowIfCancellationRequested(); _current.BiosVersion = mo["SMBIOSBIOSVersion"]?.ToString() ?? "—"; mo.Dispose(); break; }
    }

    private void DetectNetwork(CancellationToken ct)
    {
        using var s = new ManagementObjectSearcher("SELECT NetConnectionID,Speed FROM Win32_NetworkAdapter WHERE NetConnectionID IS NOT NULL AND PhysicalAdapter=TRUE");
        foreach (ManagementObject mo in s.Get()) { ct.ThrowIfCancellationRequested(); _current.NetworkAdapter = mo["NetConnectionID"]?.ToString() ?? "—"; if (mo["Speed"] is ulong sp && sp>0) _current.NetworkSpeed = $"{sp/1_000_000} Mbps"; mo.Dispose(); break; }
    }

    private void DetectOs()
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (k == null) return;
            var cb = k.GetValue("CurrentBuild")?.ToString();
            var ub = k.GetValue("UBR")?.ToString();
            _current.OsBuild = string.IsNullOrEmpty(cb) ? "—" : (string.IsNullOrEmpty(ub) ? cb : $"{cb}.{ub}");
            var edition = k.GetValue("ProductName")?.ToString() ?? "—";
            var displayVersion = k.GetValue("DisplayVersion")?.ToString();
            if (int.TryParse(cb, out var build) && build >= 22000)
            {
                // ProductName still says "Windows 10" on many Win11 SKUs (incl. IoT Enterprise LTSC).
                edition = edition.Replace("Windows 10", "Windows 11", StringComparison.OrdinalIgnoreCase);
            }
            if (!string.IsNullOrWhiteSpace(displayVersion))
                edition = $"{edition} · {displayVersion}";
            _current.OsEdition = edition;
        }
        catch { /* ignore */ }
    }

    private void DetectMonitors(CancellationToken ct)
    {
        var mons = new List<string>();

        // Count actual physical monitors via PnP, not GPU adapters
        int physicalCount = 0;
        try
        {
            using var pnp = new ManagementObjectSearcher("SELECT Name FROM Win32_PnPEntity WHERE PNPClass='Monitor'");
            foreach (ManagementObject mo in pnp.Get())
            {
                ct.ThrowIfCancellationRequested();
                physicalCount++;
                mo.Dispose();
            }
        }
        catch { /* fallback below */ }

        // Get resolution info from VideoController (one per adapter, gives active resolution)
        using var s = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution,CurrentVerticalResolution,CurrentRefreshRate FROM Win32_VideoController");
        foreach (ManagementObject mo in s.Get())
        {
            ct.ThrowIfCancellationRequested();
            var h = Convert.ToInt32(mo["CurrentHorizontalResolution"] ?? 0);
            var v = Convert.ToInt32(mo["CurrentVerticalResolution"] ?? 0);
            var r = Convert.ToInt32(mo["CurrentRefreshRate"] ?? 0);
            if (h > 0 && v > 0) mons.Add($"{h}x{v} @ {r}Hz");
            mo.Dispose();
        }

        _current.MonitorCount = Math.Max(physicalCount, 1);
        _current.Monitors = mons;
    }

    private void DetectUsb(CancellationToken ct)
    {
        var devs = new List<string>();
        try { using var s = new ManagementObjectSearcher("SELECT Name FROM Win32_PnPEntity WHERE PNPClass='USB'"); foreach(ManagementObject mo in s.Get()){ct.ThrowIfCancellationRequested(); var n=mo["Name"]?.ToString(); if (!string.IsNullOrWhiteSpace(n)) devs.Add(n!); mo.Dispose();} } catch { }
        _current.UsbDeviceCount = devs.Count; _current.UsbDevices = devs;
    }

    private void DetectAudio(CancellationToken ct)
    {
        var devs = new List<string>();
        try { using var s = new ManagementObjectSearcher("SELECT Name FROM Win32_SoundDevice"); foreach(ManagementObject mo in s.Get()){ct.ThrowIfCancellationRequested(); var n=mo["Name"]?.ToString(); if (!string.IsNullOrWhiteSpace(n)) devs.Add(n!); mo.Dispose();} } catch { }
        _current.AudioDevices = devs;
    }

    [StructLayout(LayoutKind.Sequential)] private struct SystemPowerStatus { public byte ACLineStatus; public byte BatteryFlag; public byte BatteryLifePercent; public byte SystemStatusFlag; public int BatteryLifeTime; public int BatteryFullLifeTime; }
    [DllImport("kernel32.dll")] private static extern bool GetSystemPowerStatus(ref SystemPowerStatus sps);
}

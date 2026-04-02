using System.Management;
using System.Runtime.InteropServices;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services;

public sealed class HardwareService : IHardwareService, IDisposable
{
    private readonly HardwareInfo _current = new();
    private readonly object _sync = new();
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    public HardwareInfo Current => _current;

    public event EventHandler? HardwareUpdated;

    public async Task StartAsync(CancellationToken ct = default)
    {
        await RefreshAsync(ct).ConfigureAwait(false);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _ = RunLoopAsync(_cts.Token);
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                await RefreshAsync(ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        await Task.Run(() => RefreshCore(ct), ct).ConfigureAwait(false);
    }

    private void RefreshCore(CancellationToken ct)
    {
        lock (_sync)
        {
            DetectFormFactor(ct);
            DetectPower(ct);
            DetectCpu(ct);
            DetectGpu(ct);
            DetectRam(ct);
            DetectStorage(ct);
            DetectMotherboard(ct);
            DetectNetwork(ct);
            DetectOs(ct);
            _current.Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            _current.LastUpdated = DateTime.Now.ToString("T");
        }

        HardwareUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void DetectFormFactor(CancellationToken ct)
    {
        var laptopTypes = new HashSet<ushort> { 8, 9, 10, 14, 18, 21 };
        var desktopTypes = new HashSet<ushort> { 3, 4, 5, 6, 7, 13, 15, 16, 35, 36 };
        var miniAioTypes = new HashSet<ushort> { 10, 13, 14, 15 };

        FormFactor factor = FormFactor.Unknown;
        using (var searcher = new ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                var arr = mo["ChassisTypes"] as ushort[];
                if (arr is { Length: > 0 })
                {
                    foreach (var t in arr)
                    {
                        if (laptopTypes.Contains(t))
                        {
                            factor = FormFactor.Laptop;
                            break;
                        }

                        if (miniAioTypes.Contains(t) && factor != FormFactor.Laptop)
                        {
                            factor = FormFactor.MiniPcOrAllInOne;
                        }
                        else if (desktopTypes.Contains(t) && factor == FormFactor.Unknown)
                        {
                            factor = FormFactor.Desktop;
                        }
                    }
                }

                mo.Dispose();
            }
        }

        if (factor == FormFactor.Unknown)
        {
            using var battery = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_Battery");
            using var results = battery.Get();
            foreach (ManagementObject _ in results)
            {
                factor = FormFactor.Laptop;
                break;
            }
        }

        if (factor == FormFactor.Unknown)
        {
            factor = FormFactor.Desktop;
        }

        _current.FormFactor = factor;
    }

    private void DetectPower(CancellationToken ct)
    {
        _ = ct;
        var source = PowerSource.Unknown;
        try
        {
            var sps = new SystemPowerStatus();
            if (GetSystemPowerStatus(ref sps))
            {
                if (sps.ACLineStatus == 0)
                {
                    source = PowerSource.Battery;
                }
                else if (sps.ACLineStatus == 1)
                {
                    source = PowerSource.Ac;
                }
            }
        }
        catch
        {
            source = PowerSource.Unknown;
        }

        _current.PowerSource = source;
    }

    private void DetectCpu(CancellationToken ct)
    {
        string name = "—";
        int cores = 0;
        int logical = 0;
        int maxMhz = 0;
        int curMhz = 0;
        string manufacturer = "—";

        using (var searcher = new ManagementObjectSearcher(
                   "SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, CurrentClockSpeed, Manufacturer FROM Win32_Processor"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                name = mo["Name"]?.ToString() ?? name;
                cores = Convert.ToInt32(mo["NumberOfCores"] ?? 0);
                logical = Convert.ToInt32(mo["NumberOfLogicalProcessors"] ?? 0);
                maxMhz = Convert.ToInt32(mo["MaxClockSpeed"] ?? 0);
                curMhz = Convert.ToInt32(mo["CurrentClockSpeed"] ?? 0);
                manufacturer = mo["Manufacturer"]?.ToString() ?? manufacturer;
                mo.Dispose();
                break;
            }
        }

        double util = 0;
        using (var searcher = new ManagementObjectSearcher(
                   "SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                util = Convert.ToDouble(mo["PercentProcessorTime"] ?? 0);
                mo.Dispose();
                break;
            }
        }

        double? tempC = null;
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI",
                "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                var raw = mo["CurrentTemperature"];
                if (raw is int kelvinTenths)
                {
                    tempC = (kelvinTenths / 10.0) - 273.15;
                }
                else if (raw is uint kelvinTenthsU)
                {
                    tempC = (kelvinTenthsU / 10.0) - 273.15;
                }

                mo.Dispose();
                break;
            }
        }
        catch (ManagementException)
        {
            // Thermal zone WMI class requires admin and may be unavailable.
        }

        _current.CpuName = name.Trim();
        _current.CpuCores = cores;
        _current.CpuLogicalProcessors = logical;
        _current.CpuMaxClockMhz = maxMhz;
        _current.CpuCurrentClockMhz = curMhz;
        _current.CpuManufacturer = manufacturer.Trim();
        _current.CpuUtilizationPercent = util;
        _current.CpuTemperatureC = tempC;
    }

    private void DetectGpu(CancellationToken ct)
    {
        string gpuName = "—";
        ulong ramBytes = 0;
        string driver = "—";
        string driverDate = "—";

        using (var searcher = new ManagementObjectSearcher(
                   "SELECT Name, AdapterRAM, DriverVersion, DriverDate FROM Win32_VideoController"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                var n = mo["Name"]?.ToString();
                if (string.IsNullOrWhiteSpace(n) || n!.Contains("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    mo.Dispose();
                    continue;
                }

                gpuName = n;
                ramBytes = Convert.ToUInt64(mo["AdapterRAM"] ?? 0UL);
                driver = mo["DriverVersion"]?.ToString() ?? driver;
                if (mo["DriverDate"] is string ds)
                {
                    driverDate = ManagementDateTimeConverter.ToDateTime(ds).ToString("d");
                }

                mo.Dispose();
                break;
            }
        }

        var vendor = ParseGpuVendor(gpuName);
        _current.PrimaryGpuVendor = vendor;
        _current.GpuName = gpuName;
        _current.GpuMemoryGb = ramBytes / (1024.0 * 1024.0 * 1024.0);

        if (_current.GpuMemoryGb <= 0.01)
        {
            _current.GpuMemoryGb = 0;
        }

        _current.GpuDriverVersion = driver;
        _current.GpuDriverDate = driverDate;

        double gpuUtil = 0;
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, UtilizationPercentage FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine");
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                var nm = mo["Name"]?.ToString() ?? string.Empty;
                if (nm.Contains("3D", StringComparison.OrdinalIgnoreCase) || nm.Contains("Graphics", StringComparison.OrdinalIgnoreCase))
                {
                    gpuUtil = Math.Max(gpuUtil, Convert.ToDouble(mo["UtilizationPercentage"] ?? 0));
                }

                mo.Dispose();
            }
        }
        catch (ManagementException)
        {
            // GPU engine perf counters may be unavailable on some systems.
        }

        _current.GpuUtilizationPercent = gpuUtil;

        double? gpuTemp = null;
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Temperature FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUPerformanceCounters");
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                if (mo["Temperature"] is ulong t)
                {
                    gpuTemp = t;
                }
                else if (mo["Temperature"] is uint t2)
                {
                    gpuTemp = t2;
                }

                mo.Dispose();
                break;
            }
        }
        catch (ManagementException)
        {
            // GPU perf counter WMI class may be unavailable on some systems.
        }

        _current.GpuTemperatureC = gpuTemp;
    }

    private static GpuVendor ParseGpuVendor(string name)
    {
        var n = name.ToUpperInvariant();
        if (n.Contains("NVIDIA", StringComparison.Ordinal))
        {
            return GpuVendor.Nvidia;
        }

        if (n.Contains("AMD", StringComparison.Ordinal) || n.Contains("RADEON", StringComparison.Ordinal))
        {
            return GpuVendor.Amd;
        }

        if (n.Contains("INTEL", StringComparison.Ordinal))
        {
            return GpuVendor.Intel;
        }

        return GpuVendor.Unknown;
    }

    private void DetectRam(CancellationToken ct)
    {
        double totalKb = 0;
        double freeKb = 0;

        using (var searcher = new ManagementObjectSearcher(
                   "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                totalKb = Convert.ToDouble(mo["TotalVisibleMemorySize"] ?? 0);
                freeKb = Convert.ToDouble(mo["FreePhysicalMemory"] ?? 0);
                mo.Dispose();
                break;
            }
        }

        var totalGb = totalKb / (1024.0 * 1024.0);
        var freeGb = freeKb / (1024.0 * 1024.0);
        var usedPct = totalKb > 0 ? (1.0 - (freeKb / totalKb)) * 100.0 : 0;

        _current.RamTotalGb = totalGb;
        _current.RamAvailableGb = freeGb;
        _current.RamUsedPercent = usedPct;
    }

    private void DetectStorage(CancellationToken ct)
    {
        var parts = new List<string>();
        var hasSsd = false;
        _current.ReBarLikelySupported = false;

        using (var diskSearcher = new ManagementObjectSearcher(
                   "SELECT Model, Size, MediaType FROM Win32_DiskDrive"))
        {
            foreach (ManagementObject mo in diskSearcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                var model = mo["Model"]?.ToString() ?? "Disk";
                var size = Convert.ToUInt64(mo["Size"] ?? 0UL);
                var media = mo["MediaType"]?.ToString() ?? string.Empty;
                var gb = size / (1024.0 * 1024.0 * 1024.0);
                var type = media.Contains("SSD", StringComparison.OrdinalIgnoreCase) || model.Contains("SSD", StringComparison.OrdinalIgnoreCase)
                    ? "SSD"
                    : "HDD";
                if (type == "SSD")
                {
                    hasSsd = true;
                }

                parts.Add($"{model} ({gb:0.#} GB, {type})");
                mo.Dispose();
            }
        }

        _current.HasSsdBootVolume = hasSsd;
        _current.StorageSummary = parts.Count == 0 ? "—" : string.Join("; ", parts.Take(4));

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers");
            var v = key?.GetValue("HwSchMode");
            if (v is int mode && mode == 2)
            {
                _current.ReBarLikelySupported = hasSsd;
            }
        }
        catch
        {
            // ignore
        }
    }

    private void DetectMotherboard(CancellationToken ct)
    {
        string board = "—";
        string bios = "—";

        using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                board = $"{mo["Manufacturer"]} {mo["Product"]}".Trim();
                mo.Dispose();
                break;
            }
        }

        using (var searcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion FROM Win32_BIOS"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                bios = mo["SMBIOSBIOSVersion"]?.ToString() ?? bios;
                mo.Dispose();
                break;
            }
        }

        _current.Motherboard = board;
        _current.BiosVersion = bios;
    }

    private void DetectNetwork(CancellationToken ct)
    {
        string adapter = "—";
        string speed = "—";

        using (var searcher = new ManagementObjectSearcher(
                   "SELECT NetConnectionID, Speed FROM Win32_NetworkAdapter WHERE NetConnectionID IS NOT NULL AND PhysicalAdapter = TRUE"))
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                ct.ThrowIfCancellationRequested();
                adapter = mo["NetConnectionID"]?.ToString() ?? adapter;
                if (mo["Speed"] is ulong sp && sp > 0)
                {
                    speed = $"{sp / 1_000_000} Mbps";
                }

                mo.Dispose();
                break;
            }
        }

        _current.NetworkAdapter = adapter;
        _current.NetworkSpeed = speed;
    }

    private void DetectOs(CancellationToken ct)
    {
        var build = "—";
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var cb = k?.GetValue("CurrentBuild")?.ToString();
            var ub = k?.GetValue("UBR")?.ToString();
            if (!string.IsNullOrEmpty(cb))
            {
                build = string.IsNullOrEmpty(ub) ? cb : $"{cb}.{ub}";
            }
        }
        catch
        {
            // ignore
        }

        _current.OsBuild = build;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _cts?.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemPowerStatus
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetSystemPowerStatus(ref SystemPowerStatus sps);
}

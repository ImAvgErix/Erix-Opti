using CommunityToolkit.Mvvm.ComponentModel;

namespace ErixOpti.Core.Models;

public sealed partial class HardwareInfo : ObservableObject
{
    [ObservableProperty] private FormFactor _formFactor = FormFactor.Unknown;
    [ObservableProperty] private PowerSource _powerSource = PowerSource.Unknown;
    [ObservableProperty] private string _pcName = Environment.MachineName;

    // CPU
    [ObservableProperty] private string _cpuName = "—";
    [ObservableProperty] private int _cpuCores;
    [ObservableProperty] private int _cpuLogicalProcessors;
    [ObservableProperty] private int _cpuMaxClockMhz;
    [ObservableProperty] private string _cpuManufacturer = "—";
    [ObservableProperty] private string _cpuArchitecture = "—";
    [ObservableProperty] private int _cpuL2CacheKb;
    [ObservableProperty] private int _cpuL3CacheKb;

    // GPU
    [ObservableProperty] private GpuVendor _primaryGpuVendor = GpuVendor.Unknown;
    [ObservableProperty] private string _gpuName = "—";
    [ObservableProperty] private double _gpuMemoryGb;
    [ObservableProperty] private string _gpuDriverVersion = "—";
    [ObservableProperty] private string _gpuDriverDate = "—";

    // RAM (TotalVisible = OS-visible; Installed = sum of DIMM capacity from SMBIOS)
    [ObservableProperty] private double _ramTotalGb;
    [ObservableProperty] private double _ramInstalledGb;
    [ObservableProperty] private double _ramAvailableGb;
    [ObservableProperty] private int _ramSpeedMhz;
    [ObservableProperty] private string _ramType = "—";
    [ObservableProperty] private int _ramSlotsUsed;

    // Storage
    [ObservableProperty] private bool _hasSsdBootVolume;
    [ObservableProperty] private string _storageSummary = "—";

    // Motherboard
    [ObservableProperty] private string _motherboard = "—";
    [ObservableProperty] private string _biosVersion = "—";

    // Network
    [ObservableProperty] private string _networkAdapter = "—";
    [ObservableProperty] private string _networkSpeed = "—";

    // OS
    [ObservableProperty] private string _osBuild = "—";
    [ObservableProperty] private string _osEdition = "—";
    [ObservableProperty] private TimeSpan _uptime;

    // Peripherals
    [ObservableProperty] private int _monitorCount = 1;
    [ObservableProperty] private int _usbDeviceCount;
    [ObservableProperty] private List<string> _usbDevices = [];
    [ObservableProperty] private List<string> _audioDevices = [];
    [ObservableProperty] private List<string> _monitors = [];

    // Computed
    public bool IsAmdCpu => CpuManufacturer.Contains("AMD", StringComparison.OrdinalIgnoreCase);
    public bool IsIntelCpu => CpuManufacturer.Contains("Intel", StringComparison.OrdinalIgnoreCase);
    public bool IsNvidiaGpu => PrimaryGpuVendor == GpuVendor.Nvidia;
    public bool IsDesktop => FormFactor == FormFactor.Desktop;
    public bool IsLaptop => FormFactor == FormFactor.Laptop;
    public bool HasHighRam => RamTotalGb >= 32;
    public bool HasMultiMonitor => MonitorCount >= 2;
    public bool HasLowVram => GpuMemoryGb > 0 && GpuMemoryGb <= 8;
    public bool HasManyUsb => UsbDeviceCount >= 5;
}

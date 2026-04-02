using CommunityToolkit.Mvvm.ComponentModel;

namespace ErixOpti.Core.Models;

public sealed partial class HardwareInfo : ObservableObject
{
    [ObservableProperty] private FormFactor _formFactor = FormFactor.Unknown;

    [ObservableProperty] private GpuVendor _primaryGpuVendor = GpuVendor.Unknown;

    [ObservableProperty] private PowerSource _powerSource = PowerSource.Unknown;

    [ObservableProperty] private bool _hasSsdBootVolume;

    [ObservableProperty] private bool _reBarLikelySupported;

    [ObservableProperty] private string _cpuName = "—";

    [ObservableProperty] private int _cpuCores;

    [ObservableProperty] private int _cpuLogicalProcessors;

    [ObservableProperty] private int _cpuMaxClockMhz;

    [ObservableProperty] private int _cpuCurrentClockMhz;

    [ObservableProperty] private string _cpuManufacturer = "—";

    [ObservableProperty] private double _cpuUtilizationPercent;

    [ObservableProperty] private double? _cpuTemperatureC;

    [ObservableProperty] private string _gpuName = "—";

    [ObservableProperty] private double _gpuMemoryGb;

    [ObservableProperty] private string _gpuDriverVersion = "—";

    [ObservableProperty] private string _gpuDriverDate = "—";

    [ObservableProperty] private double _gpuUtilizationPercent;

    [ObservableProperty] private double? _gpuTemperatureC;

    [ObservableProperty] private double _ramTotalGb;

    [ObservableProperty] private double _ramAvailableGb;

    [ObservableProperty] private double _ramUsedPercent;

    [ObservableProperty] private string _storageSummary = "—";

    [ObservableProperty] private string _motherboard = "—";

    [ObservableProperty] private string _biosVersion = "—";

    [ObservableProperty] private string _networkAdapter = "—";

    [ObservableProperty] private string _networkSpeed = "—";

    [ObservableProperty] private string _osBuild = "—";

    [ObservableProperty] private TimeSpan _uptime;

    [ObservableProperty] private string _lastUpdated = "—";
}

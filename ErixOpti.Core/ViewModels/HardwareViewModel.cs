using CommunityToolkit.Mvvm.ComponentModel;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.ViewModels;

public sealed partial class HardwareViewModel : ObservableObject
{
    public HardwareViewModel(IHardwareService hardware) => Model = hardware.Current;
    public HardwareInfo Model { get; }

    public string RamTotalFormatted => $"{Model.RamTotalGb:0.#}";

    public string L3Display => Model.CpuL3CacheKb >= 1024 ? $"{Model.CpuL3CacheKb / 1024} MB" : $"{Model.CpuL3CacheKb} KB";
    public string MonitorSummary => Model.Monitors.Count == 0 ? "—" : string.Join(", ", Model.Monitors);
    public string AudioSummary => Model.AudioDevices.Count == 0 ? "None" : string.Join(", ", Model.AudioDevices.Take(2));
}

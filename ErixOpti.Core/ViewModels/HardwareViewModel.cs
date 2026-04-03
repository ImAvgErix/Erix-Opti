using CommunityToolkit.Mvvm.ComponentModel;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.ViewModels;

public sealed partial class HardwareViewModel : ObservableObject
{
    public HardwareViewModel(IHardwareService hardware)
    {
        Model = hardware.Current;
        Model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is null
                or nameof(HardwareInfo.RamTotalGb)
                or nameof(HardwareInfo.RamInstalledGb)
                or nameof(HardwareInfo.RamType)
                or nameof(HardwareInfo.RamSpeedMhz)
                or nameof(HardwareInfo.RamSlotsUsed))
            {
                OnPropertyChanged(nameof(RamHeadlineGb));
                OnPropertyChanged(nameof(RamSubline));
                OnPropertyChanged(nameof(RamTotalFormatted));
            }
            if (e.PropertyName is null or nameof(HardwareInfo.GpuMemoryGb))
                OnPropertyChanged(nameof(GpuVramGbFormatted));
        };
    }

    public HardwareInfo Model { get; }

    /// <summary>Installed DIMM total when available; otherwise OS-visible RAM.</summary>
    public string RamHeadlineGb => Model.RamInstalledGb >= 0.5 ? $"{Model.RamInstalledGb:0.#}" : $"{Model.RamTotalGb:0.#}";

    public string RamSubline
    {
        get
        {
            var baseLine = $"{Model.RamType} · {Model.RamSpeedMhz} MHz · {Model.RamSlotsUsed} stick{(Model.RamSlotsUsed == 1 ? "" : "s")}";
            if (Model.RamInstalledGb < 0.5) return baseLine;
            var diff = Math.Abs(Model.RamInstalledGb - Model.RamTotalGb);
            return diff >= 0.12 ? $"{baseLine} · {Model.RamTotalGb:0.#} GB visible to OS" : baseLine;
        }
    }

    public string RamTotalFormatted => $"{Model.RamTotalGb:0.#}";

    public string GpuVramGbFormatted => Model.GpuMemoryGb > 0 ? $"{Model.GpuMemoryGb:0.#}" : "—";

    public string L3Display => Model.CpuL3CacheKb >= 1024 ? $"{Model.CpuL3CacheKb / 1024} MB" : $"{Model.CpuL3CacheKb} KB";
    public string MonitorSummary => Model.Monitors.Count == 0 ? "—" : string.Join(", ", Model.Monitors);
    public string AudioSummary => Model.AudioDevices.Count == 0 ? "None" : string.Join(", ", Model.AudioDevices.Take(2));
}

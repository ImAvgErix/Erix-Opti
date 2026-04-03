using System.Text;
using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage(HardwareViewModel hw, OptimizationsViewModel opt)
    {
        HW = hw;
        Opt = opt;
        InitializeComponent();
        Loaded += (_, _) => Opt.LoadSummary();
    }

    public HardwareViewModel HW { get; }
    public OptimizationsViewModel Opt { get; }

    private async void OnExport(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var h = HW.Model;
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"erixopti-hw-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        var sb = new StringBuilder();
        sb.AppendLine("ErixOpti Hardware Report").AppendLine($"{DateTime.Now:F}").AppendLine();
        sb.AppendLine($"PC: {h.PcName} | {h.OsEdition} Build {h.OsBuild} | {h.FormFactor}");
        sb.AppendLine($"CPU: {h.CpuName} ({h.CpuCores}C/{h.CpuLogicalProcessors}T, {h.CpuMaxClockMhz} MHz, L3 {HW.L3Display})");
        sb.AppendLine($"GPU: {h.GpuName} ({h.GpuMemoryGb:0.#} GB VRAM, Driver {h.GpuDriverVersion})");
        sb.AppendLine($"RAM: {h.RamTotalGb:0.#} GB {h.RamType} @ {h.RamSpeedMhz} MHz ({h.RamSlotsUsed} sticks)");
        sb.AppendLine($"Storage: {h.StorageSummary}");
        sb.AppendLine($"Board: {h.Motherboard} | BIOS: {h.BiosVersion}");
        sb.AppendLine($"Network: {h.NetworkAdapter} ({h.NetworkSpeed})");
        sb.AppendLine($"Monitors: {h.MonitorCount} | USB: {h.UsbDeviceCount} | Audio: {HW.AudioSummary}");
        await File.WriteAllTextAsync(path, sb.ToString());
    }
}

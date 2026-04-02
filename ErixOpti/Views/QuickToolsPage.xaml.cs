using System.Text;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class QuickToolsPage : Page
{
    private readonly IHardwareService _hardware;

    public QuickToolsPage(QuickToolsViewModel viewModel, IHardwareService hardware)
    {
        ViewModel = viewModel;
        _hardware = hardware;
        InitializeComponent();
    }

    public QuickToolsViewModel ViewModel { get; }

    private void OnOpenUrl(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string url)
        {
            ViewModel.OpenUrlCommand.Execute(url);
        }
    }

    private void OnLaunchPath(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string path)
        {
            ViewModel.OpenExecutableCommand.Execute(path);
        }
    }

    private void OnControlPanel(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string applet)
        {
            ViewModel.OpenControlPanelCommand.Execute(applet);
        }
    }

    private async void OnCreateRestorePoint(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var backup = App.AppHost.Services.GetRequiredService<IBackupService>();

        var progress = new Progress<string>(s => ViewModel.StatusMessage = s);
        var result = await backup.CreateFullBackupAsync(progress, CancellationToken.None);
        ViewModel.StatusMessage = result.Success ? "Restore point / backup completed." : result.Error ?? "Failed.";
    }

    private async void OnExportHardwareReport(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            $"erixopti-hardware-{DateTime.Now:yyyyMMdd-HHmmss}.txt");

        var h = _hardware.Current;
        var sb = new StringBuilder();
        sb.AppendLine("════════════════════════════════════════════════════════");
        sb.AppendLine("  ErixOpti — Full Hardware Report");
        sb.AppendLine($"  Generated: {DateTime.Now:F}");
        sb.AppendLine("════════════════════════════════════════════════════════");
        sb.AppendLine();

        sb.AppendLine("─── SYSTEM ───────────────────────────────────────────");
        sb.AppendLine($"  Form factor      : {h.FormFactor}");
        sb.AppendLine($"  Power source     : {h.PowerSource}");
        sb.AppendLine($"  OS build         : {h.OsBuild}");
        sb.AppendLine($"  Uptime           : {h.Uptime}");
        sb.AppendLine();

        sb.AppendLine("─── PROCESSOR ────────────────────────────────────────");
        sb.AppendLine($"  Name             : {h.CpuName}");
        sb.AppendLine($"  Manufacturer     : {h.CpuManufacturer}");
        sb.AppendLine($"  Cores / threads  : {h.CpuCores} / {h.CpuLogicalProcessors}");
        sb.AppendLine($"  Max clock (MHz)  : {h.CpuMaxClockMhz}");
        sb.AppendLine($"  Cur clock (MHz)  : {h.CpuCurrentClockMhz}");
        sb.AppendLine($"  Utilization      : {h.CpuUtilizationPercent:F1}%");
        sb.AppendLine($"  Temperature      : {(h.CpuTemperatureC.HasValue ? $"{h.CpuTemperatureC.Value:F1} °C" : "N/A")}");
        sb.AppendLine();

        sb.AppendLine("─── GRAPHICS ─────────────────────────────────────────");
        sb.AppendLine($"  Name             : {h.GpuName}");
        sb.AppendLine($"  Vendor           : {h.PrimaryGpuVendor}");
        sb.AppendLine($"  VRAM (GB)        : {h.GpuMemoryGb:F1}");
        sb.AppendLine($"  Driver version   : {h.GpuDriverVersion}");
        sb.AppendLine($"  Driver date      : {h.GpuDriverDate}");
        sb.AppendLine($"  Utilization      : {h.GpuUtilizationPercent:F1}%");
        sb.AppendLine($"  Temperature      : {(h.GpuTemperatureC.HasValue ? $"{h.GpuTemperatureC.Value:F0} °C" : "N/A")}");
        sb.AppendLine();

        sb.AppendLine("─── MEMORY ───────────────────────────────────────────");
        sb.AppendLine($"  Total (GB)       : {h.RamTotalGb:F2}");
        sb.AppendLine($"  Available (GB)   : {h.RamAvailableGb:F2}");
        sb.AppendLine($"  Used             : {h.RamUsedPercent:F1}%");
        sb.AppendLine();

        sb.AppendLine("─── STORAGE ──────────────────────────────────────────");
        sb.AppendLine($"  Disks            : {h.StorageSummary}");
        sb.AppendLine($"  SSD boot volume  : {h.HasSsdBootVolume}");
        sb.AppendLine($"  ReBAR likely     : {h.ReBarLikelySupported}");
        sb.AppendLine();

        sb.AppendLine("─── MOTHERBOARD ──────────────────────────────────────");
        sb.AppendLine($"  Board            : {h.Motherboard}");
        sb.AppendLine($"  BIOS version     : {h.BiosVersion}");
        sb.AppendLine();

        sb.AppendLine("─── NETWORK ──────────────────────────────────────────");
        sb.AppendLine($"  Adapter          : {h.NetworkAdapter}");
        sb.AppendLine($"  Speed            : {h.NetworkSpeed}");
        sb.AppendLine();
        sb.AppendLine("════════════════════════════════════════════════════════");

        await File.WriteAllTextAsync(path, sb.ToString());
        ViewModel.StatusMessage = $"Hardware report saved to {path}";
    }
}

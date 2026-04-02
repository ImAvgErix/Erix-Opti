using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ErixOpti.Core.ViewModels;

public sealed partial class QuickToolsViewModel : ObservableObject
{
    [ObservableProperty] private string _statusMessage = "Ready.";

    [RelayCommand]
    private void OpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            StatusMessage = $"Opening {url}…";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenExecutable(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            StatusMessage = $"Launching {path}…";
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenControlPanel(string? applet)
    {
        if (string.IsNullOrWhiteSpace(applet))
        {
            return;
        }

        try
        {
            if (applet.EndsWith(".msc", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo("mmc.exe", applet) { UseShellExecute = true });
            }
            else
            {
                Process.Start(new ProcessStartInfo("control.exe", applet) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }
}

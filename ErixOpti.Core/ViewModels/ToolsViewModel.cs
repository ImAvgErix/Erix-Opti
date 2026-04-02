using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Services.Tweaks;

namespace ErixOpti.Core.ViewModels;

public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly IBackupService _backup;
    public ToolsViewModel(IBackupService backup) => _backup = backup;

    [ObservableProperty] private string _statusMessage = "Ready.";
    [ObservableProperty] private double _cleanupProgress;
    [ObservableProperty] private bool _isCleaning;
    public bool IsNotCleaning => !IsCleaning;
    partial void OnIsCleaningChanged(bool value) => OnPropertyChanged(nameof(IsNotCleaning));

    [RelayCommand]
    private async Task DeepCleanAsync()
    {
        if (IsCleaning) return; IsCleaning = true; CleanupProgress = 0;
        var ops = CleanupTweaks.All;
        for (int i = 0; i < ops.Count; i++)
        {
            StatusMessage = ops[i].Name; CleanupProgress = (double)(i+1)/ops.Count*100;
            try { var p = new Progress<string>(s => StatusMessage = s); await ops[i].Apply(p, CancellationToken.None); }
            catch (Exception ex) { StatusMessage = $"{ops[i].Name} failed: {ex.Message}"; }
        }
        StatusMessage = "Deep cleanup complete."; CleanupProgress = 100; IsCleaning = false;
    }

    [RelayCommand]
    private async Task CreateRestorePointAsync()
    {
        StatusMessage = "Creating restore point...";
        var p = new Progress<string>(s => StatusMessage = s);
        var r = await _backup.CreateFullBackupAsync(p, CancellationToken.None);
        StatusMessage = r.Success ? "Restore point created." : $"Failed: {r.Error}";
    }

    [RelayCommand]
    private void OpenPanel(string? applet)
    {
        if (string.IsNullOrWhiteSpace(applet)) return;
        try { if (applet.EndsWith(".msc",StringComparison.OrdinalIgnoreCase)) Process.Start(new ProcessStartInfo("mmc.exe",applet){UseShellExecute=true}); else Process.Start(new ProcessStartInfo("control.exe",applet){UseShellExecute=true}); }
        catch (Exception ex) { StatusMessage = ex.Message; }
    }
}

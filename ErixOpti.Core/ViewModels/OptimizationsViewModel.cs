using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services;

namespace ErixOpti.Core.ViewModels;

public sealed partial class OptimizationsViewModel : ObservableObject
{
    private readonly IAutoOptimizeEngine _engine;
    private readonly IBackupService _backup;
    private readonly IHardwareService _hw;

    public OptimizationsViewModel(IAutoOptimizeEngine engine, IBackupService backup, IHardwareService hw)
    { _engine = engine; _backup = backup; _hw = hw; }

    [ObservableProperty] private string _statusMessage = "Ready to optimize.";
    [ObservableProperty] private string _currentStep = "";
    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _hardwareSummary = "";

    public bool IsNotRunning => !IsRunning;
    partial void OnIsRunningChanged(bool value) => OnPropertyChanged(nameof(IsNotRunning));

    public void LoadSummary()
    {
        var h = _hw.Current;
        HardwareSummary = $"{h.CpuName}  ·  {h.GpuName}  ·  {h.RamTotalGb:0.#} GB RAM";
    }

    [RelayCommand]
    private async Task AutoOptimizeAsync()
    {
        if (IsRunning) return;
        IsRunning = true; ProgressPercent = 0; CurrentStep = "";
        try
        {
            StatusMessage = "Creating backup...";
            var bp = new Progress<string>(s => StatusMessage = s);
            var br = await _backup.CreateFullBackupAsync(bp, CancellationToken.None);
            if (!br.Success) { StatusMessage = $"Backup failed: {br.Error}"; return; }

            StatusMessage = "Building plan...";
            var plan = await _engine.BuildPlanAsync(CancellationToken.None);
            StatusMessage = $"Applying {plan.TotalCount} optimizations...";

            var progress = new Progress<OptimizeProgress>(p =>
            {
                ProgressPercent = (double)p.Current / p.Total * 100.0;
                CurrentStep = p.StepName;
                StatusMessage = p.Detail;
            });
            await _engine.ExecutePlanAsync(plan, progress, CancellationToken.None);
            StatusMessage = $"Done — {plan.TotalCount} optimizations applied.";
            ProgressPercent = 100;
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        finally { IsRunning = false; }
    }
}

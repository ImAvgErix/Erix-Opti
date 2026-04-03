using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly IOptimizeSessionState _session;
    private readonly IUserDialogService _dialogs;

    public OptimizationsViewModel(
        IAutoOptimizeEngine engine,
        IBackupService backup,
        IHardwareService hw,
        IOptimizeSessionState session,
        IUserDialogService dialogs)
    {
        _engine = engine;
        _backup = backup;
        _hw = hw;
        _session = session;
        _dialogs = dialogs;
        TweakCategories = new ObservableCollection<TweakCategoryVm>();
        PlannedDecisions = new ObservableCollection<PlannedDecisionRowVm>();
        LastRunResults = new ObservableCollection<LastOptimizeStepVm>();
    }

    public ObservableCollection<TweakCategoryVm> TweakCategories { get; }

    public ObservableCollection<PlannedDecisionRowVm> PlannedDecisions { get; }

    public ObservableCollection<LastOptimizeStepVm> LastRunResults { get; }

    [ObservableProperty] private string _statusMessage = "Ready to optimize.";

    [ObservableProperty] private string _currentStep = "";

    [ObservableProperty] private double _progressPercent;

    [ObservableProperty] private bool _isRunning;

    [ObservableProperty] private string _hardwareSummary = "";

    [ObservableProperty] private string _tweakStatsLine = "";

    public bool IsNotRunning => !IsRunning;

    partial void OnIsRunningChanged(bool value) => OnPropertyChanged(nameof(IsNotRunning));

    public async Task RefreshDashboardAsync(CancellationToken ct = default)
    {
        var h = _hw.Current;
        HardwareSummary = $"{h.CpuName}  ·  {h.GpuName}  ·  {h.RamTotalGb:0.#} GB RAM";

        PlannedDecisions.Clear();
        foreach (var pt in HardwareDecisionEngine.BuildPlan(h))
        {
            PlannedDecisions.Add(new PlannedDecisionRowVm(pt.Operation.Name, pt.DecisionReason));
        }

        await TweakListBuilder.RebuildAsync(TweakCategories, h, ct);
        var (active, eligible, total) = await TweakListBuilder.CountSummaryAsync(h, ct);
        TweakStatsLine = eligible > 0
            ? $"{active} of {eligible} detectable tweaks already active  ·  {total} total in catalog"
            : $"{total} tweaks in catalog";

        LastRunResults.Clear();
        foreach (var s in _session.LastRunSteps)
        {
            LastRunResults.Add(new LastOptimizeStepVm(s.Name, s.Reason, s.Ok));
        }
    }

    [RelayCommand]
    private async Task AutoOptimizeAsync()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        ProgressPercent = 0;
        CurrentStep = "";
        try
        {
            StatusMessage = "Creating backup...";
            var bp = new Progress<string>(s => StatusMessage = s);
            var br = await _backup.CreateFullBackupAsync(bp, CancellationToken.None);
            if (!br.Success)
            {
                StatusMessage = $"Backup failed: {br.Error}";
                return;
            }

            StatusMessage = "Building plan...";
            var plan = await _engine.BuildPlanAsync(CancellationToken.None);
            if (plan.TotalCount == 0)
            {
                StatusMessage = "No tweaks apply to this PC.";
                return;
            }

            StatusMessage = $"Applying {plan.TotalCount} optimizations...";

            var progress = new Progress<OptimizeProgress>(p =>
            {
                ProgressPercent = (double)p.Current / Math.Max(p.Total, 1) * 100.0;
                CurrentStep = p.StepName;
                StatusMessage = p.Detail;
            });
            await _engine.ExecutePlanAsync(plan, progress, CancellationToken.None);
            ProgressPercent = 100;

            LastRunResults.Clear();
            foreach (var s in _session.LastRunSteps)
            {
                LastRunResults.Add(new LastOptimizeStepVm(s.Name, s.Reason, s.Ok));
            }

            if (_session.RebootRecommended && await _dialogs.PromptRestartAfterOptimizeAsync())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/r /t 0",
                    UseShellExecute = true,
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            await RefreshDashboardAsync();
        }
    }
}

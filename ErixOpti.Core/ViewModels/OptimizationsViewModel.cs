using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services;

namespace ErixOpti.Core.ViewModels;

public sealed partial class OptimizationsViewModel : ObservableObject
{
    private readonly ITweakCatalog _catalog;
    private readonly TweakApplyService _applyService;
    private readonly IBackupService _backupService;
    private readonly IUserDialogService _dialogs;
    private bool _sessionBackupCompleted;

    public OptimizationsViewModel(
        ITweakCatalog catalog,
        TweakApplyService applyService,
        IBackupService backupService,
        IUserDialogService dialogs)
    {
        _catalog = catalog;
        _applyService = applyService;
        _backupService = backupService;
        _dialogs = dialogs;
        foreach (var t in _catalog.All.OrderBy(x => x.Category).ThenBy(x => x.Name))
        {
            Items.Add(new TweakItemViewModel(t));
        }
    }

    public ObservableCollection<TweakItemViewModel> Items { get; } = new();

    [RelayCommand]
    private async Task ApplyPresetAsync(string? presetName)
    {
        if (!Enum.TryParse<TweakPreset>(presetName, out var preset))
        {
            return;
        }

        StatusMessage = $"Applying preset {preset}…";
        var tweaks = _catalog.GetByPreset(preset);
        foreach (var tweak in tweaks)
        {
            await ApplyCoreAsync(tweak, requireHighRiskConfirm: true).ConfigureAwait(true);
        }

        StatusMessage = "Preset finished.";
    }

    [RelayCommand]
    public async Task ApplyAsync(TweakItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        await ApplyCoreAsync(item.Tweak, requireHighRiskConfirm: true).ConfigureAwait(true);
    }

    [RelayCommand]
    public async Task RevertAsync(TweakItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        try
        {
            StatusMessage = $"Reverting {item.Tweak.Name}…";
            var progress = new Progress<string>(s => StatusMessage = s);
            await item.Tweak.RevertAsync(progress, CancellationToken.None).ConfigureAwait(true);
            item.IsApplied = await item.Tweak.IsAppliedAsync(CancellationToken.None).ConfigureAwait(true);
            StatusMessage = "Revert complete.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Revert failed: {ex.Message}";
        }
    }

    public async Task RefreshStatesAsync()
    {
        foreach (var item in Items)
        {
            await item.RefreshAppliedAsync().ConfigureAwait(true);
        }
    }

    private async Task ApplyCoreAsync(ITweak tweak, bool requireHighRiskConfirm)
    {
        try
        {
            if (!await tweak.IsApplicableAsync(CancellationToken.None).ConfigureAwait(true))
            {
                StatusMessage = $"{tweak.Name} is not applicable on this configuration.";
                return;
            }

            if (requireHighRiskConfirm && tweak.Risk == RiskLevel.High)
            {
                if (!await _dialogs.ConfirmHighRiskTweakAsync(tweak).ConfigureAwait(true))
                {
                    StatusMessage = "Cancelled.";
                    return;
                }
            }

            if (!_sessionBackupCompleted)
            {
                if (!await _dialogs.ConfirmBackupBeforeTweaksAsync().ConfigureAwait(true))
                {
                    StatusMessage = "Backup declined — apply cancelled.";
                    return;
                }

                var progress = new Progress<string>(s => StatusMessage = s);
                var result = await _backupService.CreateFullBackupAsync(progress, CancellationToken.None).ConfigureAwait(true);
                if (!result.Success)
                {
                    StatusMessage = $"Backup failed: {result.Error}";
                    return;
                }

                _sessionBackupCompleted = true;
            }

            var applyProgress = new Progress<string>(s => StatusMessage = s);
            await _applyService.ApplySafeAsync(tweak, applyProgress, CancellationToken.None).ConfigureAwait(true);

            var vm = Items.First(i => i.Tweak.Id == tweak.Id);
            vm.IsApplied = await tweak.IsAppliedAsync(CancellationToken.None).ConfigureAwait(true);
            StatusMessage = $"{tweak.Name} applied.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Apply failed: {ex.Message}";
        }
    }

    [ObservableProperty] private string _statusMessage = "Ready.";
}

public sealed partial class TweakItemViewModel : ObservableObject
{
    public TweakItemViewModel(ITweak tweak)
    {
        Tweak = tweak;
    }

    public ITweak Tweak { get; }

    [ObservableProperty] private bool _isApplied;

    public async Task RefreshAppliedAsync()
    {
        IsApplied = await Tweak.IsAppliedAsync(CancellationToken.None).ConfigureAwait(true);
    }
}

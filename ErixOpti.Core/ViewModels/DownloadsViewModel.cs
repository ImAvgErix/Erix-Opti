using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services;

namespace ErixOpti.Core.ViewModels;

public sealed partial class DownloadsViewModel : ObservableObject
{
    public DownloadsViewModel(IDownloadManager dl)
    {
        foreach (var item in DownloadCatalog.All)
            Items.Add(new DownloadItemViewModel(item, dl));
    }

    public ObservableCollection<DownloadItemViewModel> Items { get; } = new();
}

public sealed partial class DownloadItemViewModel : ObservableObject
{
    private readonly IDownloadManager _dl;

    public DownloadItemViewModel(DownloadItem item, IDownloadManager dl)
    {
        Item = item;
        _dl = dl;
    }

    public DownloadItem Item { get; }

    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private string _speedText = "";
    [ObservableProperty] private string _downloadedPath = "";
    [ObservableProperty] private string _errorText = "";

    public bool IsWebOnly => !Item.IsDirectDownload;

    partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(ShowError));

    private DownloadState _state = DownloadState.Ready;
    public DownloadState State
    {
        get => _state;
        set
        {
            if (SetProperty(ref _state, value))
            {
                OnPropertyChanged(nameof(ShowProgress));
                OnPropertyChanged(nameof(ShowError));
                OnPropertyChanged(nameof(ShowInstall));
                OnPropertyChanged(nameof(DownloadButtonText));
                OnPropertyChanged(nameof(IsDownloadEnabled));
            }
        }
    }

    public bool ShowProgress => State is DownloadState.Downloading;
    public bool ShowError => State is DownloadState.Failed && !string.IsNullOrEmpty(ErrorText);
    public bool ShowInstall => State is DownloadState.Downloaded;
    public bool IsDownloadEnabled => State is DownloadState.Ready or DownloadState.Failed;

    public string DownloadButtonText => State switch
    {
        DownloadState.Downloading => "Downloading…",
        DownloadState.Downloaded => "Downloaded",
        DownloadState.Installing => "Installing…",
        DownloadState.Installed => "Installed",
        DownloadState.Failed => "Retry",
        _ => IsWebOnly ? "Open page" : "Download",
    };

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (!Item.IsDirectDownload) return; // handled by UI opening WebView2
        if (State is DownloadState.Downloading or DownloadState.Installing) return;

        State = DownloadState.Downloading;
        ProgressPercent = 0;
        ErrorText = "";

        try
        {
            var progress = new Progress<DownloadProgress>(dp =>
            {
                if (dp.TotalBytes > 0)
                    ProgressPercent = (double)dp.BytesReceived / dp.TotalBytes * 100.0;
                SpeedText = $"{dp.SpeedMBps:0.0} MB/s";
            });

            DownloadedPath = await _dl.DownloadAsync(Item, progress, CancellationToken.None);
            State = DownloadState.Downloaded;
            ProgressPercent = 100;
        }
        catch (Exception ex)
        {
            State = DownloadState.Failed;
            ErrorText = ex.Message;
        }
    }

    [RelayCommand]
    private async Task InstallAsync()
    {
        if (State != DownloadState.Downloaded || string.IsNullOrEmpty(DownloadedPath)) return;
        State = DownloadState.Installing;
        ErrorText = "";

        try
        {
            await _dl.InstallAsync(DownloadedPath, Item.SilentArgs, CancellationToken.None);
            State = DownloadState.Installed;
        }
        catch (Exception ex)
        {
            State = DownloadState.Failed;
            ErrorText = ex.Message;
        }
    }
}

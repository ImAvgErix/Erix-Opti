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

    [ObservableProperty] private DownloadState _state = DownloadState.Ready;
    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private string _speedText = "";
    [ObservableProperty] private string _downloadedPath = "";
    [ObservableProperty] private string _errorText = "";

    public bool IsWebOnly => !Item.IsDirectDownload;

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

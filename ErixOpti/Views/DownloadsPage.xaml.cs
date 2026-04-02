using ErixOpti.Core.Models;
using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class DownloadsPage : Page
{
    public DownloadsPage(DownloadsViewModel vm) { VM = vm; InitializeComponent(); }
    public DownloadsViewModel VM { get; }

    public static Visibility ShowIf(DownloadState s, int kind) => kind switch
    {
        1 => s == DownloadState.Downloading ? Visibility.Visible : Visibility.Collapsed,
        2 => s == DownloadState.Downloaded ? Visibility.Visible : Visibility.Collapsed,
        _ => Visibility.Collapsed
    };

    public static string BtnText(DownloadState s) => s switch
    {
        DownloadState.Ready => "Download",
        DownloadState.Downloading => "...",
        DownloadState.Downloaded => "Done",
        DownloadState.Installed => "Installed",
        DownloadState.Failed => "Retry",
        _ => "Download"
    };
}

using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class DownloadsPage : Page
{
    public DownloadsPage(DownloadsViewModel vm) { VM = vm; InitializeComponent(); }
    public DownloadsViewModel VM { get; }
}

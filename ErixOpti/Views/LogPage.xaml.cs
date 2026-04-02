using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class LogPage : Page
{
    public LogPage(LogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public LogViewModel ViewModel { get; }
}

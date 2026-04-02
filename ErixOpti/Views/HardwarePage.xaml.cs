using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class HardwarePage : Page
{
    public HardwarePage(HardwareViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public HardwareViewModel ViewModel { get; }
}

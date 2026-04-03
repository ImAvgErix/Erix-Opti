using ErixOpti.Core.Interfaces;
using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class CatalogPage : Page
{
    private readonly IHardwareService _hw;

    public CatalogPage(OptimizationsViewModel vm, IHardwareService hw)
    {
        VM = vm;
        _hw = hw;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public OptimizationsViewModel VM { get; }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await _hw.RefreshAsync();
        await VM.RefreshDashboardAsync();
    }
}

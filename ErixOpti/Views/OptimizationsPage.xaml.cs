using ErixOpti.Core.Models;
using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ErixOpti.Views;

public sealed partial class OptimizationsPage : Page
{
    public OptimizationsPage(OptimizationsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public OptimizationsViewModel ViewModel { get; }

    public static string RiskGlyph(RiskLevel risk) => risk switch
    {
        RiskLevel.Low => "\uE930",
        RiskLevel.Medium => "\uE7BA",
        RiskLevel.High => "\uE814",
        _ => "\uE946"
    };

    public static Brush RiskBrush(RiskLevel risk) => risk switch
    {
        RiskLevel.Low => (Brush)Application.Current.Resources["RiskLowBrush"],
        RiskLevel.Medium => (Brush)Application.Current.Resources["RiskMediumBrush"],
        RiskLevel.High => (Brush)Application.Current.Resources["RiskHighBrush"],
        _ => (Brush)Application.Current.Resources["AppSubtleTextBrush"]
    };

    public static string AppliedLabel(bool applied) =>
        applied ? "\u2705 Applied" : "\u2B1C Not applied";

    public static string RebootLabel(bool reboot) =>
        reboot ? "  ·  Reboot required" : string.Empty;

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshStatesAsync();
    }

    private async void OnApplyClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TweakItemViewModel vm)
        {
            await ViewModel.ApplyAsync(vm);
        }
    }

    private async void OnRevertClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TweakItemViewModel vm)
        {
            await ViewModel.RevertAsync(vm);
        }
    }

    private async void OnPresetClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string tag)
        {
            await ViewModel.ApplyPresetCommand.ExecuteAsync(tag);
        }
    }
}

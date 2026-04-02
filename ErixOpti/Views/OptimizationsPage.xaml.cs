using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class OptimizationsPage : Page
{
    public OptimizationsPage(OptimizationsViewModel vm) { VM = vm; InitializeComponent(); Loaded += (_, _) => VM.LoadSummary(); }
    public OptimizationsViewModel VM { get; }
}

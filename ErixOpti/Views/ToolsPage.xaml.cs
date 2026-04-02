using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class ToolsPage : Page
{
    public ToolsPage(ToolsViewModel vm) { VM = vm; InitializeComponent(); }
    public ToolsViewModel VM { get; }
    private void OnCpl(object sender, RoutedEventArgs e) { if (sender is Button b && b.Tag is string t) VM.OpenPanelCommand.Execute(t); }
}

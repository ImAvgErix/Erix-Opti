using ErixOpti.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Views;

public sealed partial class LogPage : Page
{
    public LogPage(LogViewModel vm) { VM = vm; InitializeComponent(); }
    public LogViewModel VM { get; }
}

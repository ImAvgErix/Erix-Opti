using ErixOpti.Services;
using ErixOpti.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti;

public sealed partial class MainWindow : Window
{
    private readonly WindowContext _wc;
    private readonly UiLogSink _sink;
    private bool _init;

    public MainWindow(WindowContext wc, UiLogSink sink)
    {
        InitializeComponent();
        Title = "ErixOpti";
        ExtendsContentIntoTitleBar = true;
        _wc = wc; _sink = sink;
        Activated += OnActivated;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        if (_init) return; _init = true;
        _wc.XamlRoot = Content.XamlRoot;
        _sink.Attach(DispatcherQueue);
        Nav.SelectedItem = Nav.MenuItems[0];
    }

    private void OnNavChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
            Navigate(tag);
    }

    private void Navigate(string tag)
    {
        var svc = App.AppHost.Services;
        ContentFrame.Content = tag switch
        {
            "hw" => svc.GetRequiredService<HardwarePage>(),
            "opt" => svc.GetRequiredService<OptimizationsPage>(),
            "dl" => svc.GetRequiredService<DownloadsPage>(),
            "tools" => svc.GetRequiredService<ToolsPage>(),
            "log" => svc.GetRequiredService<LogPage>(),
            _ => svc.GetRequiredService<HardwarePage>()
        };
    }
}

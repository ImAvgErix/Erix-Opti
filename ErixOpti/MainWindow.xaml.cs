using ErixOpti.Services;
using ErixOpti.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
namespace ErixOpti;

public sealed partial class MainWindow : Window
{
    private readonly WindowContext _windowContext;
    private readonly UiLogSink _uiLogSink;
    private bool _initialized;

    public MainWindow(WindowContext windowContext, UiLogSink uiLogSink)
    {
        InitializeComponent();
        Title = "ErixOpti";
        ExtendsContentIntoTitleBar = true;
        _windowContext = windowContext;
        _uiLogSink = uiLogSink;

        Activated += OnActivated;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        _windowContext.XamlRoot = Content.XamlRoot;
        _uiLogSink.Attach(DispatcherQueue);
        RootNav.SelectedItem = RootNav.MenuItems[0];
        Navigate("hardware");
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            Navigate(tag);
        }
    }

    private void Navigate(string tag)
    {
        var services = App.AppHost.Services;
        Page page = tag switch
        {
            "hardware" => services.GetRequiredService<HardwarePage>(),
            "optimizations" => services.GetRequiredService<OptimizationsPage>(),
            "quick" => services.GetRequiredService<QuickToolsPage>(),
            "log" => services.GetRequiredService<LogPage>(),
            _ => services.GetRequiredService<HardwarePage>()
        };

        ContentFrame.Content = page;
    }
}

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
        ContentFrame.Content = App.AppHost.Services.GetRequiredService<DashboardPage>();
    }
}

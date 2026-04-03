using System;
using ErixOpti.Services;
using ErixOpti.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
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
        _wc = wc;
        _sink = sink;
        Activated += OnActivated;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        if (_init) return;
        _init = true;
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is not null)
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(dq));

        _wc.XamlRoot = Content.XamlRoot;
        _sink.Attach(DispatcherQueue);

        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void OnNavChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        var tag = item.Tag?.ToString();
        ContentFrame.Content = tag switch
        {
            "Home" => App.AppHost.Services.GetRequiredService<HomePage>(),
            "Catalog" => App.AppHost.Services.GetRequiredService<CatalogPage>(),
            "Log" => App.AppHost.Services.GetRequiredService<LogPage>(),
            _ => App.AppHost.Services.GetRequiredService<HomePage>(),
        };
    }
}

using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Services;
using ErixOpti.Core.ViewModels;
using ErixOpti.Services;
using ErixOpti.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;

namespace ErixOpti;

public partial class App : Application
{
    private Window? _window;
    public App() => InitializeComponent();
    public static IHost AppHost { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (!Core.Helpers.AdminHelper.IsRunningAsAdministrator()) { _ = Core.Helpers.AdminHelper.TryRestartElevated(); Exit(); return; }

        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ErixOpti", "logs", "app-.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        var logVm = new LogViewModel();
        var uiSink = new UiLogSink(logVm);

        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().Enrich.FromLogContext()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day).WriteTo.Sink(uiSink).CreateLogger();

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureLogging(lb => { lb.ClearProviders(); lb.AddSerilog(Log.Logger, dispose: true); })
            .ConfigureServices(s =>
            {
                s.AddSingleton<WindowContext>();
                s.AddSingleton<IUserDialogService, UserDialogService>();
                s.AddSingleton<LogViewModel>(_ => logVm);
                s.AddSingleton(uiSink);
                s.AddSingleton<IHardwareService, HardwareService>();
                s.AddSingleton<IBackupService, BackupService>();
                s.AddSingleton<IAutoOptimizeEngine, AutoOptimizeEngine>();
                s.AddSingleton<IDownloadManager, DownloadManager>();
                s.AddSingleton<HardwareViewModel>();
                s.AddSingleton<OptimizationsViewModel>();
                s.AddSingleton<DownloadsViewModel>();
                s.AddSingleton<ToolsViewModel>();
                s.AddSingleton<MainWindow>();
                s.AddTransient<HardwarePage>();
                s.AddTransient<OptimizationsPage>();
                s.AddTransient<DownloadsPage>();
                s.AddTransient<ToolsPage>();
                s.AddTransient<LogPage>();
            }).Build();

        await AppHost.Services.GetRequiredService<IHardwareService>().StartAsync();
        _window = AppHost.Services.GetRequiredService<MainWindow>();
        _window.Activate();
    }
}

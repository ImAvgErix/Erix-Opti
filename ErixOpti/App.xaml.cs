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

    public App()
    {
        InitializeComponent();
    }

    public static IHost AppHost { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (!ErixOpti.Core.Helpers.AdminHelper.IsRunningAsAdministrator())
        {
            _ = ErixOpti.Core.Helpers.AdminHelper.TryRestartElevated();
            Exit();
            return;
        }

        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ErixOpti",
            "logs",
            "app-.log");

        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        var logVm = new LogViewModel();
        var uiSink = new UiLogSink(logVm);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Sink(uiSink)
            .CreateLogger();

        AppHost = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddSerilog(Log.Logger, dispose: true);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<WindowContext>();
                services.AddSingleton<IUserDialogService, UserDialogService>();
                services.AddSingleton<LogViewModel>(_ => logVm);
                services.AddSingleton(uiSink);
                services.AddSingleton<IHardwareService, HardwareService>();
                services.AddSingleton<IBackupService, BackupService>();
                services.AddSingleton<TweakApplyService>();
                services.AddSingleton<ITweakCatalog>(sp => new TweakCatalog(sp.GetRequiredService<IHardwareService>()));
                services.AddSingleton<HardwareViewModel>();
                services.AddSingleton<OptimizationsViewModel>();
                services.AddSingleton<QuickToolsViewModel>();
                services.AddSingleton<MainWindow>();
                services.AddTransient<HardwarePage>();
                services.AddTransient<OptimizationsPage>();
                services.AddTransient<QuickToolsPage>();
                services.AddTransient<LogPage>();
            })
            .Build();

        var hardwareService = AppHost.Services.GetRequiredService<IHardwareService>();
        await hardwareService.StartAsync();

        _window = AppHost.Services.GetRequiredService<MainWindow>();
        _window.Activate();
    }
}

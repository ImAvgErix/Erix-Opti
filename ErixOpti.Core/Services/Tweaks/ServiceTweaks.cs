using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public static class ServiceTweaks
{
    private static readonly (string Name, string Desc)[] DisableList =
    [
        ("WSearch", "Windows Search indexer"),
        ("SysMain", "Superfetch/SysMain prefetching"),
        ("DoSvc", "Delivery Optimization P2P"),
        ("Spooler", "Print Spooler"),
        ("Fax", "Fax service"),
        ("lfsvc", "Geolocation service"),
        ("WerSvc", "Windows Error Reporting"),
        ("DPS", "Diagnostic Policy Service"),
        ("RetailDemo", "Retail Demo service"),
        ("SCardSvr", "Smart Card service"),
        ("XblAuthManager", "Xbox Live Auth Manager"),
        ("XblGameSave", "Xbox Live Game Save"),
        ("XboxNetApiSvc", "Xbox Net API"),
        ("WdiServiceHost", "Diagnostic Service Host"),
        ("WdiSystemHost", "Diagnostic System Host"),
        ("AxInstSV", "ActiveX Installer"),
        ("DiagTrack", "Connected User Telemetry"),
        ("PcaSvc", "Program Compatibility Assistant"),
        ("WalletService", "Wallet Service"),
        ("tzautoupdate", "Auto Time Zone Updater"),
        ("PhoneSvc", "Phone Service"),
        ("seclogon", "Secondary Logon"),
        ("TabletInputService", "Tablet Input Service"),
        ("SmsRouter", "SMS Router"),
        ("icssvc", "Internet Connection Sharing"),
        ("MapsBroker", "Downloaded Maps Manager"),
        ("WpnService", "Push Notifications"),
        ("PushToInstall", "Push to Install service"),
        ("WaaSMedicSvc", "Windows Update Medic"),
        ("WarpJITSvc", "WarpJIT service"),
        ("wisvc", "Windows Insider service"),
        ("dmwappushservice", "WAP Push service"),
        ("RemoteRegistry", "Remote Registry"),
        ("SharedAccess", "Internet Connection Sharing"),
        ("TrkWks", "Distributed Link Tracking"),
        ("WMPNetworkSvc", "WMP Network Sharing"),
        ("AJRouter", "AllJoyn Router"),
        ("ALG", "Application Layer Gateway"),
        ("BITS", "Background Intelligent Transfer"),
        ("CertPropSvc", "Certificate Propagation"),
        ("diagnosticshub.standardcollector.service", "Diagnostics Hub Collector"),
        // Hyper-V guest services (desktop, no VMs)
        ("vmickvpexchange", "Hyper-V Data Exchange"),
        ("vmicguestinterface", "Hyper-V Guest Interface"),
        ("vmicshutdown", "Hyper-V Guest Shutdown"),
        ("vmicheartbeat", "Hyper-V Heartbeat"),
        ("vmictimesync", "Hyper-V Time Sync"),
        ("vmicvss", "Hyper-V Volume Shadow Copy"),
        ("vmicrdv", "Hyper-V Remote Desktop"),
        ("vmicvmsession", "Hyper-V PowerShell Direct"),
        // Remote Desktop
        ("SessionEnv", "Remote Desktop Configuration"),
        ("TermService", "Remote Desktop Services"),
        ("UmRdpService", "Remote Desktop UserMode Port"),
        // Sensors (desktop)
        ("SensorService", "Sensor Service"),
        ("SensrSvc", "Sensor Monitoring"),
        ("SensorDataService", "Sensor Data Service"),
        // Other
        ("ScDeviceEnum", "Smart Card Device Enumeration"),
        ("SCPolicySvc", "Smart Card Removal Policy"),
        ("RasAuto", "Remote Access Auto Connection"),
        ("RasMan", "Remote Access Connection Manager"),
        ("WpcMonSvc", "Parental Controls"),
        ("DialogBlockingService", "Dialog Blocking"),
        ("GraphicsPerfSvc", "Graphics Performance Monitor"),
    ];

    public static IReadOnlyList<TweakOperation> All
    {
        get
        {
            var ops = new List<TweakOperation>();
            foreach (var (svc, desc) in DisableList)
            {
                var s = svc;
                ops.Add(new TweakOperation
                {
                    Id = $"svc.{s.ToLowerInvariant()}", Name = $"Disable {s}", Category = "Services",
                    Description = desc,
                    ShouldApply = _ => true,
                    TryGetAppliedState = _ => ServiceProbeHelper.IsServiceDisabled(s),
                    Apply = async (p, ct) => { p.Report($"Disabling {s}"); await ProcessRunner.RunAsync("sc.exe", $"config \"{s}\" start= disabled", false, null, ct); await ProcessRunner.RunAsync("sc.exe", $"stop \"{s}\"", false, null, ct); },
                    Revert = async (p, ct) => { p.Report($"Restore {s}"); await ProcessRunner.RunAsync("sc.exe", $"config \"{s}\" start= demand", false, null, ct); }
                });
            }
            ops.Add(new TweakOperation
            {
                Id = "svc.bth-manual", Name = "Bluetooth to Manual", Category = "Services",
                Description = "Sets Bluetooth service to manual start.",
                ShouldApply = _ => true,
                TryGetAppliedState = _ => ServiceProbeHelper.RegistryStartIs("bthserv", 3),
                Apply = async (p, ct) => { p.Report("Bluetooth → Manual"); await ProcessRunner.RunAsync("sc.exe", "config bthserv start= demand", false, null, ct); },
                Revert = async (p, ct) => { await ProcessRunner.RunAsync("sc.exe", "config bthserv start= auto", false, null, ct); }
            });
            ops.Add(new TweakOperation
            {
                Id = "svc.memcompress", Name = "Memory compression off", Category = "Services",
                Description = "Disables memory compression (high-RAM systems).",
                ShouldApply = hw => hw.HasHighRam,
                TryGetAppliedState = _ => null,
                Apply = async (p, ct) => { p.Report("Disable memory compression"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Disable-MMAgent -MemoryCompression\"", false, null, ct); },
                Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Enable-MMAgent -MemoryCompression\"", false, null, ct); }
            });
            return ops;
        }
    }
}

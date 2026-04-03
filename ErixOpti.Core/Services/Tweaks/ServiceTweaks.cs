using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class ServiceTweaks
{
    private static bool? ServiceStartIs(string serviceName, int startValue)
    {
        try
        {
            using var bk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var k = bk.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
            if (k?.GetValue("Start") is not int s)
                return null;
            return s == startValue;
        }
        catch
        {
            return null;
        }
    }

    private static readonly string[] DisableList =
    [
        "WSearch","SysMain","DoSvc","Spooler","Fax","lfsvc","WerSvc","DPS",
        "RetailDemo","SCardSvr","XblAuthManager","XblGameSave","XboxNetApiSvc",
        "WdiServiceHost","WdiSystemHost","AxInstSV","DiagTrack","PcaSvc",
        "WalletService","tzautoupdate","PhoneSvc","seclogon","TabletInputService",
        "SmsRouter","icssvc","MapsBroker","WpnService","PushToInstall",
        "WaaSMedicSvc","WarpJITSvc","wisvc","dmwappushservice","RemoteRegistry",
        "SharedAccess","TrkWks","WMPNetworkSvc","AJRouter","ALG",
        "BITS","CertPropSvc","diagnosticshub.standardcollector.service"
    ];

    public static IReadOnlyList<TweakOperation> All
    {
        get
        {
            var ops = new List<TweakOperation>();
            foreach (var svc in DisableList)
            {
                var s = svc;
                ops.Add(new TweakOperation
                {
                    Id = $"svc.{s.ToLowerInvariant()}", Name = $"Disable {s}", Category = "Services",
                    ShouldApply = _ => true,
                    TryGetAppliedState = _ => ServiceStartIs(s, 4),
                    Apply = async (p, ct) => { p.Report($"Disabling {s}"); await ProcessRunner.RunAsync("sc.exe", $"config {s} start= disabled", false, null, ct); await ProcessRunner.RunAsync("sc.exe", $"stop {s}", false, null, ct); },
                    Revert = async (p, ct) => { p.Report($"Restore {s}"); await ProcessRunner.RunAsync("sc.exe", $"config {s} start= demand", false, null, ct); }
                });
            }
            ops.Add(new TweakOperation
            {
                Id = "svc.bth-manual", Name = "Bluetooth to Manual", Category = "Services",
                ShouldApply = _ => true,
                TryGetAppliedState = _ => ServiceStartIs("bthserv", 3),
                Apply = async (p, ct) => { p.Report("Bluetooth → Manual"); await ProcessRunner.RunAsync("sc.exe", "config bthserv start= demand", false, null, ct); },
                Revert = async (p, ct) => { await ProcessRunner.RunAsync("sc.exe", "config bthserv start= auto", false, null, ct); }
            });
            ops.Add(new TweakOperation
            {
                Id = "svc.memcompress", Name = "Memory compression off", Category = "Services",
                ShouldApply = hw => hw.HasHighRam,
                TryGetAppliedState = _ => null,
                Apply = async (p, ct) => { p.Report("Disable memory compression"); await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Disable-MMAgent -MemoryCompression\"", false, null, ct); },
                Revert = async (p, ct) => { await ProcessRunner.RunAsync("powershell.exe", "-NoProfile -Command \"Enable-MMAgent -MemoryCompression\"", false, null, ct); }
            });
            return ops;
        }
    }
}

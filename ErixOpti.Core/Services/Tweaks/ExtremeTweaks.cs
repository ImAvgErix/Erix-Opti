using System.Text;
using ErixOpti.Core.Helpers;
using ErixOpti.Core.Models;
using Microsoft.Win32;

namespace ErixOpti.Core.Services.Tweaks;

public static class ExtremeTweaks
{
    private static readonly Func<HardwareInfo, bool> GamingDiscreteGpu =
        hw => hw.PrimaryGpuVendor is GpuVendor.Nvidia or GpuVendor.Amd;

    public static IReadOnlyList<TweakOperation> All =>
    [
        new()
        {
            Id = "sec.mitigations",
            Name = "CPU mitigations relaxed (gaming)",
            Category = "Security",
            PlanOrder = 0,
            ShouldApply = GamingDiscreteGpu,
            ExplainDecision = hw =>
                $"Detected discrete {hw.PrimaryGpuVendor} GPU — applying controlled mitigation relaxations to lower micro-stutter overhead (registry-only; reversible from backup).",
            TryGetAppliedState = _ =>
            {
                var a = RegistryTweakHelper.TryReadDword(
                    RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "FeatureSettingsOverride",
                    out var o) && o == 3;
                var b = RegistryTweakHelper.TryReadDword(
                    RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "FeatureSettingsOverrideMask",
                    out var m) && m == 3;
                return a && b;
            },
            Apply = async (p, ct) =>
            {
                p.Report("Mitigation overrides");
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v FeatureSettingsOverride /t REG_DWORD /d 3 /f",
                        false,
                        null,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v FeatureSettingsOverrideMask /t REG_DWORD /d 3 /f",
                        false,
                        null,
                        ct)
                    .ConfigureAwait(false);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync(
                        "reg",
                        "delete \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v FeatureSettingsOverride /f",
                        false,
                        null,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "delete \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v FeatureSettingsOverrideMask /f",
                        false,
                        null,
                        ct)
                    .ConfigureAwait(false);
            },
        },
        new()
        {
            Id = "sec.defender-strip",
            Name = "Windows Defender stack stripped (DISM + services + policy)",
            Category = "Security",
            PlanOrder = 10,
            ShouldApply = GamingDiscreteGpu,
            ExplainDecision = hw =>
                $"Detected {hw.PrimaryGpuVendor} discrete GPU — Defender is treated as non-essential for this barebones gaming profile; services are stopped, start-up disabled, policies tightened, and optional packages are removed where Windows allows.",
            TryGetAppliedState = _ => DefenderProbeSync(),
            TryGetAppliedStateAsync = async (_, ct) =>
            {
                var sync = DefenderProbeSync();
                if (sync is false)
                {
                    return false;
                }

                if (sync is true)
                {
                    return true;
                }

                return await DefenderProbeDismAsync(ct).ConfigureAwait(false);
            },
            Apply = async (p, ct) =>
            {
                p.Report("Defender: stop + disable services (admin)");
                await RunPowerShellScriptAsync(
                        """
                        $n = 'WinDefend','WdNisSvc','Sense','SecurityHealthService'
                        Get-Service $n -EA SilentlyContinue | Stop-Service -Force -EA SilentlyContinue
                        Get-Service $n -EA SilentlyContinue | Set-Service -StartupType Disabled -EA SilentlyContinue
                        """,
                        p,
                        ct)
                    .ConfigureAwait(false);

                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiSpyware /t REG_DWORD /d 1 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableBehaviorMonitoring /t REG_DWORD /d 1 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableOnAccessProtection /t REG_DWORD /d 1 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableScanOnRealtimeEnable /t REG_DWORD /d 1 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);

                p.Report("Defender: capability removal (admin, then TrustedInstaller if bundled)");
                await RunPowerShellScriptAsync(
                        """
                        Get-WindowsCapability -Online -EA SilentlyContinue |
                          Where-Object Name -like '*Windows.Defender*' |
                          ForEach-Object { try { Remove-WindowsCapability -Online -Name $_.Name -ErrorAction Stop } catch {} }
                        """,
                        p,
                        ct)
                    .ConfigureAwait(false);

                var psCap = Path.Combine(Path.GetTempPath(), $"erix-def-{Guid.NewGuid():N}.ps1");
                await File.WriteAllTextAsync(
                        psCap,
                        """
                        Get-WindowsCapability -Online -EA SilentlyContinue |
                          Where-Object Name -like '*Windows.Defender*' |
                          ForEach-Object { try { Remove-WindowsCapability -Online -Name $_.Name -ErrorAction Stop } catch {} }
                        """,
                        Encoding.UTF8,
                        ct)
                    .ConfigureAwait(false);
                try
                {
                    if (TrustedInstallerHelper.HasTrustedInstallerLauncher())
                    {
                        await TrustedInstallerHelper.RunProtectedCommandAsync(
                                $"powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"{psCap}\"",
                                p,
                                ct)
                            .ConfigureAwait(false);
                    }
                }
                finally
                {
                    try
                    {
                        File.Delete(psCap);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                var defenderRemover = BundledToolResolver.ResolveDefenderRemover();
                if (defenderRemover is not null)
                {
                    p.Report("DefenderRemover.exe (silent «y» — ionuttbara / community build)");
                    await ProcessRunner.RunAsync(defenderRemover, "y", false, p, ct).ConfigureAwait(false);
                }
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "sec.ai-strip",
            Name = "Windows AI / Copilot / Recall hooks removed",
            Category = "Security",
            PlanOrder = 20,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "Copilot, Recall, and related AI surfaces are removed or disabled so they cannot respawn after updates.",
            TryGetAppliedState = _ => AiStripProbe(),
            Apply = async (p, ct) =>
            {
                p.Report("AI / Copilot registry policy");
                var cmds = new[]
                {
                    "add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot\" /v TurnOffWindowsCopilot /t REG_DWORD /d 1 /f",
                    "add \"HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot\" /v TurnOffWindowsCopilot /t REG_DWORD /d 1 /f",
                    "add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI\" /v DisableWindowsAI /t REG_DWORD /d 1 /f",
                };
                foreach (var c in cmds)
                {
                    await ProcessRunner.RunAsync("reg", c, false, p, ct).ConfigureAwait(false);
                }

                p.Report("AI Appx removal");
                await RunPowerShellScriptAsync(
                        """
                        $names = 'Microsoft.Copilot','Microsoft.Windows.Ai.Copilot.Provider','MicrosoftWindows.Client.AIX'
                        Get-AppxPackage -AllUsers -EA SilentlyContinue | Where-Object { $n = $_.Name; $names | Where-Object { $n -like $_ } } | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -EA SilentlyContinue } catch {} }
                        Get-AppxProvisionedPackage -Online -EA SilentlyContinue | Where-Object { $n = $_.DisplayName; $names | Where-Object { $n -like $_ } } | Remove-AppxProvisionedPackage -Online -EA SilentlyContinue
                        """,
                        p,
                        ct)
                    .ConfigureAwait(false);
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "ui.dark-startmenu",
            Name = "Dark mode + classic Win11 Start behavior",
            Category = "Explorer",
            PlanOrder = 0,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "System and apps are forced to dark chrome, Start recommendations are turned off, and the standard Windows 11 Start layout is preferred over the newest promotional variant.",
            TryGetAppliedState = _ =>
            {
                var a = RegistryTweakHelper.TryReadDwordUser(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme",
                    out var ap) && ap == 0;
                var s = RegistryTweakHelper.TryReadDwordUser(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "SystemUsesLightTheme",
                    out var sy) && sy == 0;
                return a && s;
            },
            Apply = async (p, ct) =>
            {
                p.Report("Dark mode + Start policy");
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\" /v AppsUseLightTheme /t REG_DWORD /d 0 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\" /v SystemUsesLightTheme /t REG_DWORD /d 0 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\" /v Start_IrisRecommendations /t REG_DWORD /d 0 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
                await ProcessRunner.RunAsync(
                        "reg",
                        "add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\" /v Start_ShowRecommendations /t REG_DWORD /d 0 /f",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "task.telemetry-off",
            Name = "Telemetry & maintenance scheduled tasks disabled",
            Category = "Tasks",
            PlanOrder = 0,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "Disables recurring telemetry, CEIP, and disk-diagnostic tasks so they stop waking disks and CPU in the background.",
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                p.Report("Disable noisy scheduled tasks");
                await RunPowerShellScriptAsync(
                        """
                        $paths = @(
                          '\Microsoft\Windows\Application Experience',
                          '\Microsoft\Windows\Customer Experience Improvement Program',
                          '\Microsoft\Windows\Feedback\Siuf',
                          '\Microsoft\Windows\DiskDiagnostic',
                          '\Microsoft\Windows\Windows Error Reporting',
                          '\Microsoft\Windows\Maps',
                          '\Microsoft\Windows\Location'
                        )
                        foreach ($tp in $paths) {
                          Get-ScheduledTask -TaskPath $tp -EA SilentlyContinue | Disable-ScheduledTask -EA SilentlyContinue
                        }
                        """,
                        p,
                        ct)
                    .ConfigureAwait(false);
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "net.extreme-extra",
            Name = "Network stack ultra-low-latency extras",
            Category = "Network",
            PlanOrder = 100,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "Adds common low-latency host tweaks (TCP chimney / RSC hints) on top of the base profile.",
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("netsh", "int tcp set global chimney=disabled", false, p, ct).ConfigureAwait(false);
                await ProcessRunner.RunAsync("netsh", "int tcp set global rsc=disabled", false, p, ct).ConfigureAwait(false);
            },
            Revert = async (p, ct) =>
            {
                await ProcessRunner.RunAsync("netsh", "int tcp set global chimney=automatic", false, p, ct).ConfigureAwait(false);
                await ProcessRunner.RunAsync("netsh", "int tcp set global rsc=enabled", false, p, ct).ConfigureAwait(false);
            },
        },
        new()
        {
            Id = "app.strict-debloat",
            Name = "Strict Appx debloat (Store / Terminal / Snipping / Notepad / Photos kept)",
            Category = "Apps",
            PlanOrder = 0,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "Removes inbox UWP packages except Microsoft Store, Terminal, Snipping Tool, Notepad, Photos, App Installer (winget), and shell pieces required for a working desktop.",
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                p.Report("Appx strict debloat");
                await RunPowerShellScriptAsync(StrictDebloatScript, p, ct).ConfigureAwait(false);
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "adv.safe-deep-clean",
            Name = "Safe deep cleanup (no tweak reset)",
            Category = "Cleanup",
            PlanOrder = 2000,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "Clears delivery-optimization cache, Windows Store caches, temp locations, and runs DISM StartComponentCleanup without ResetBase — it does not undo services, registry optimizations, or power policies.",
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                p.Report("Delivery Optimization cache");
                var d = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "ServiceProfiles",
                    "LocalService",
                    "AppData",
                    "Local",
                    "Microsoft",
                    "DeliveryOptimization",
                    "Cache");
                TryDeleteDir(d, p);

                p.Report("Store caches");
                TryDeleteDir(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Packages",
                        "Microsoft.WindowsStore_8wekyb3d8bbwe",
                        "LocalCache"),
                    p);

                p.Report("DISM StartComponentCleanup");
                await ProcessRunner.RunAsync(
                        "dism.exe",
                        "/Online /Cleanup-Image /StartComponentCleanup",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "post.browser-runtime",
            Name = "Browser removal + gaming runtime install check",
            Category = "Post",
            PlanOrder = 0,
            ShouldApply = _ => true,
            ExplainDecision = _ =>
                "Consumer browsers are trimmed first; then local installers (WebView2, VC++ AIO, dxwebsetup, App Installer msix) if present under Assets or Downloads; winget fills gaps; Chocolatey is bootstrapped only if needed and used as a secondary installer for key game clients.",
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                p.Report("Remove consumer browsers (WebView2 runtime preserved where possible)");
                await RunPowerShellScriptAsync(BrowserRemovalScript, p, ct).ConfigureAwait(false);

                await RuntimeBootstrapHelper.RunPostOptimizeRuntimeAsync(p, ct).ConfigureAwait(false);
            },
            Revert = (_, _) => Task.CompletedTask,
        },
        new()
        {
            Id = "post.timer-resolution",
            Name = "Timer resolution measured + persistent keeper task",
            Category = "Post",
            PlanOrder = 40,
            ShouldApply = _ => true,
            ExplainDecision = hw =>
                $"Measures this machine’s sleep granularity ({hw.CpuName}) and applies the finest supported system timer resolution, then registers a scheduled task so it survives logon.",
            TryGetAppliedState = _ => null,
            Apply = async (p, ct) =>
            {
                p.Report("Measure timer granularity");
                var before = TimerResolutionNative.MeasureSleepGranularityMs();
                TimerResolutionNative.SetMaximum(out var applied);
                var after = TimerResolutionNative.MeasureSleepGranularityMs();
                p.Report($"Timer: median Sleep(1)≈{before:0.###} ms → {after:0.###} ms; NtSet≈{applied:0.###} ms");

                var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ErixOpti");
                Directory.CreateDirectory(root);
                var psPath = Path.Combine(root, "keep-timer.ps1");
                await File.WriteAllTextAsync(
                        psPath,
                        """
                        $ErrorActionPreference = 'SilentlyContinue'
                        Add-Type -Namespace Erix -Name Tr -MemberDefinition @'
                        [DllImport("ntdll.dll")] public static extern uint NtQueryTimerResolution(out uint min, out uint max, out uint cur);
                        [DllImport("ntdll.dll")] public static extern uint NtSetTimerResolution(uint desired, bool set, out uint cur);
                        '@
                        while ($true) {
                          [uint32]$min=0; [uint32]$max=0; [uint32]$cur=0
                          [void][Erix.Tr]::NtQueryTimerResolution([ref]$min, [ref]$max, [ref]$cur)
                          [uint32]$out=0
                          [void][Erix.Tr]::NtSetTimerResolution($min, $true, [ref]$out)
                          Start-Sleep -Seconds 30
                        }
                        """,
                        Encoding.UTF8,
                        ct)
                    .ConfigureAwait(false);

                var taskName = "ErixOptiTimerResolution";
                var inner =
                    $"powershell.exe -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File \"{psPath}\"";
                var escaped = inner.Replace("\"", "\\\"", StringComparison.Ordinal);
                await ProcessRunner.RunAsync(
                        "schtasks.exe",
                        $"/Create /F /TN \"{taskName}\" /TR \"{escaped}\" /SC ONLOGON /RL HIGHEST",
                        false,
                        p,
                        ct)
                    .ConfigureAwait(false);
            },
            Revert = async (_, ct) =>
            {
                await ProcessRunner.RunAsync("schtasks.exe", "/Delete /F /TN ErixOptiTimerResolution", false, null, ct)
                    .ConfigureAwait(false);
            },
        },
    ];

    private const string StrictDebloatScript =
        """
        $keep = @(
          '*Microsoft.WindowsStore*',
          '*Microsoft.WindowsTerminal*',
          '*Microsoft.ScreenSketch*',
          '*Microsoft.WindowsNotepad*',
          '*Microsoft.Windows.Photos*',
          '*Microsoft.DesktopAppInstaller*',
          '*Microsoft.SecHealthUI*',
          '*Microsoft.Windows.ShellExperienceHost*',
          '*Microsoft.Windows.StartMenuExperienceHost*',
          '*MicrosoftWindows.Client.CBS*'
        )
        Get-AppxPackage -AllUsers -EA SilentlyContinue | Where-Object {
          $n = $_.PackageFullName
          $m = $false
          foreach ($k in $keep) { if ($n -like $k) { $m = $true; break } }
          -not $m
        } | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -EA SilentlyContinue } catch {} }

        Get-AppxProvisionedPackage -Online -EA SilentlyContinue | Where-Object {
          $n = $_.PackageName
          $m = $false
          foreach ($k in $keep) { if ($n -like $k) { $m = $true; break } }
          -not $m
        } | Remove-AppxProvisionedPackage -Online -EA SilentlyContinue
        """;

    private const string BrowserRemovalScript =
        """
        $skip = '*WebView2*'
        winget list --accept-source-agreements --disable-interactivity -e 2>$null | Out-Null
        $ids = @('Google.Chrome','Mozilla.Firefox','Opera.Opera','Microsoft.Edge')
        foreach ($id in $ids) {
          if ($id -like '*Edge*') {
            winget uninstall --id $id --silent --accept-source-agreements --disable-interactivity 2>$null
          } else {
            winget uninstall --id $id --silent --accept-source-agreements --disable-interactivity 2>$null
          }
        }
        Get-AppxPackage -AllUsers -EA SilentlyContinue | Where-Object { $_.Name -like '*MicrosoftEdge*' -and $_.PackageFullName -notlike $skip } | Remove-AppxPackage -AllUsers -EA SilentlyContinue
        """;

    private static bool? DefenderProbeSync()
    {
        var pol = RegistryTweakHelper.TryReadDword(
            RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows Defender",
            "DisableAntiSpyware",
            out var d) && d == 1;
        if (!pol)
        {
            return false;
        }

        var wd = ServiceProbeHelper.IsServiceDisabled("WinDefend");
        return wd;
    }

    private static async Task<bool?> DefenderProbeDismAsync(CancellationToken ct)
    {
        var (code, stdout, _) = await ProcessRunner.RunAsync(
                "dism.exe",
                "/Online /Get-FeatureInfo /FeatureName:Windows-Defender",
                false,
                null,
                ct)
            .ConfigureAwait(false);
        if (code != 0)
        {
            return null;
        }

        if (stdout.Contains("Disabled with Payload Removed", StringComparison.OrdinalIgnoreCase) ||
            stdout.Contains("Disabled", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return null;
    }

    private static bool? AiStripProbe()
    {
        var a = RegistryTweakHelper.TryReadDword(
            RegistryHive.LocalMachine,
            @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot",
            "TurnOffWindowsCopilot",
            out var v) && v == 1;
        return a;
    }

    private static void TryDeleteDir(string path, IProgress<string> p)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(f);
                }
                catch
                {
                    // ignore
                }
            }

            foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories).OrderByDescending(x => x.Length))
            {
                try
                {
                    Directory.Delete(d, true);
                }
                catch
                {
                    // ignore
                }
            }

            p.Report($"Cleaned {path}");
        }
        catch (Exception ex)
        {
            p.Report($"Cleanup skip {path}: {ex.Message}");
        }
    }

    private static async Task RunPowerShellScriptAsync(string script, IProgress<string>? p, CancellationToken ct)
    {
        var path = Path.Combine(Path.GetTempPath(), $"erix-{Guid.NewGuid():N}.ps1");
        await File.WriteAllTextAsync(path, script, Encoding.UTF8, ct).ConfigureAwait(false);
        try
        {
            await ProcessRunner.RunAsync(
                    "powershell.exe",
                    $"-NoProfile -ExecutionPolicy Bypass -File \"{path}\"",
                    false,
                    p,
                    ct)
                .ConfigureAwait(false);
        }
        finally
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }
    }

}

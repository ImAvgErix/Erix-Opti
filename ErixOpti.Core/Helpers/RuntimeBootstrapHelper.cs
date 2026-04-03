using System.Text;

namespace ErixOpti.Core.Helpers;

/// <summary>
/// Winget from local msix, Chocolatey bootstrap, and silent runs for common redistributable installers.
/// </summary>
public static class RuntimeBootstrapHelper
{
    private const string ChocolateyInstallScript = """
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
""";

    public static async Task RunPostOptimizeRuntimeAsync(IProgress<string> p, CancellationToken ct)
    {
        p.Report(BundledToolResolver.SummarizeFoundTools());

        await EnsureWingetFromLocalBundleAsync(p, ct).ConfigureAwait(false);

        await InstallLocalSilentBootstrappersAsync(p, ct).ConfigureAwait(false);

        p.Report("Winget: gaming dependencies (primary)");
        await WingetEnsureAsync(
                [
                    ("Brave.Brave", "Brave"),
                    ("Valve.Steam", "Steam"),
                    ("Discord.Discord", "Discord"),
                    ("EpicGames.EpicGamesLauncher", "Epic Games Launcher"),
                    ("Microsoft.VCRedist.2015+.x64", "VC++ 2015+ x64"),
                    ("Microsoft.VCRedist.2015+.x86", "VC++ 2015+ x86"),
                    ("Microsoft.DirectX", "DirectX"),
                    ("Microsoft.DotNet.DesktopRuntime.8", ".NET 8 Desktop Runtime"),
                ],
                p,
                ct)
            .ConfigureAwait(false);

        await EnsureChocolateyThenFillGapsAsync(p, ct).ConfigureAwait(false);
    }

    public static async Task EnsureWingetFromLocalBundleAsync(IProgress<string> p, CancellationToken ct)
    {
        if (await IsWingetOnPathAsync(ct).ConfigureAwait(false))
        {
            p.Report("Winget already available.");
            return;
        }

        var bundle = BundledToolResolver.ResolveDesktopAppInstallerBundle();
        if (bundle is null)
        {
            p.Report("Winget: no local DesktopAppInstaller .msixbundle in Assets/Downloads — skipping offline install.");
            return;
        }

        p.Report($"Winget: installing from {Path.GetFileName(bundle)}");
        var lit = bundle.Replace("'", "''", StringComparison.Ordinal);
        await RunPowerShellScriptAsync(
                $"Add-AppxPackage -LiteralPath '{lit}' -ErrorAction Stop",
                p,
                ct)
            .ConfigureAwait(false);
    }

    public static async Task InstallLocalSilentBootstrappersAsync(IProgress<string> p, CancellationToken ct)
    {
        var wv = BundledToolResolver.ResolveWebView2Bootstrapper();
        if (wv is not null)
        {
            p.Report("WebView2 Runtime (local silent)");
            await ProcessRunner.RunAsync(wv, "/silent /install", false, p, ct).ConfigureAwait(false);
        }

        var dx = BundledToolResolver.ResolveDirectXWebSetup();
        if (dx is not null)
        {
            p.Report("DirectX web setup (local /Q)");
            await ProcessRunner.RunAsync(dx, "/Q", false, p, ct).ConfigureAwait(false);
        }

        var vc = BundledToolResolver.ResolveVcRedistAio();
        if (vc is not null)
        {
            p.Report("Visual C++ AIO (local /ai silent all-in-one)");
            await ProcessRunner.RunAsync(vc, "/ai", false, p, ct).ConfigureAwait(false);
        }
    }

    private static async Task EnsureChocolateyThenFillGapsAsync(IProgress<string> p, CancellationToken ct)
    {
        var choco = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "chocolatey",
            "choco.exe");
        if (!File.Exists(choco))
        {
            p.Report("Chocolatey: not installed — bootstrapping (official install.ps1, process scope only)…");
            await RunPowerShellScriptAsync(ChocolateyInstallScript, p, ct).ConfigureAwait(false);
        }

        if (!File.Exists(choco))
        {
            choco = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "chocolatey",
                "choco.exe");
        }

        if (!File.Exists(choco))
        {
            p.Report("Chocolatey: install did not produce choco.exe — skipping choco fallback.");
            return;
        }

        p.Report("Chocolatey: secondary pass for common game clients (skips if already satisfied)…");
        foreach (var id in new[] { "brave", "steam", "discord", "epicgameslauncher" })
        {
            ct.ThrowIfCancellationRequested();
            await ProcessRunner.RunAsync(
                    choco,
                    $"install {id} -y --no-progress --limit-output",
                    false,
                    p,
                    ct)
                .ConfigureAwait(false);
        }
    }

    private static async Task<bool> IsWingetOnPathAsync(CancellationToken ct)
    {
        var (code, _, _) = await ProcessRunner.RunAsync("where.exe", "winget", false, null, ct).ConfigureAwait(false);
        return code == 0;
    }

    private static async Task RunPowerShellScriptAsync(string script, IProgress<string>? p, CancellationToken ct)
    {
        var path = Path.Combine(Path.GetTempPath(), $"erix-rt-{Guid.NewGuid():N}.ps1");
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

    private static async Task WingetEnsureAsync(
        IReadOnlyList<(string Id, string Label)> packages,
        IProgress<string> p,
        CancellationToken ct)
    {
        if (!await IsWingetOnPathAsync(ct).ConfigureAwait(false))
        {
            p.Report("Winget not on PATH — skipped package checks (install App Installer bundle).");
            return;
        }

        foreach (var (id, label) in packages)
        {
            ct.ThrowIfCancellationRequested();
            p.Report($"winget: check {label}");
            var (listCode, listOut, _) = await ProcessRunner.RunAsync(
                    "winget.exe",
                    $"list --id {id} --accept-source-agreements --disable-interactivity",
                    false,
                    null,
                    ct)
                .ConfigureAwait(false);
            if (listCode == 0 && listOut.Contains(id, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await ProcessRunner.RunAsync(
                    "winget.exe",
                    $"install --id {id} -e --accept-package-agreements --accept-source-agreements --silent --disable-interactivity",
                    false,
                    p,
                    ct)
                .ConfigureAwait(false);
        }
    }
}

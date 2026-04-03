namespace ErixOpti.Core.Helpers;

/// <summary>
/// Resolves bundled tools only from directories shipped with the app (<c>Assets/</c>, <c>Assets/Tools/</c>).
/// Place executables and installers there before build/publish so every machine has the same payloads.
/// Optional dev override: set environment variable <c>ERIXOPTI_TOOLS_DIR</c> to an extra search root.
/// </summary>
public static class BundledToolResolver
{
    public const string ToolsOverrideEnvVar = "ERIXOPTI_TOOLS_DIR";

    public static IEnumerable<string> SearchRoots()
    {
        var extra = Environment.GetEnvironmentVariable(ToolsOverrideEnvVar);
        if (!string.IsNullOrWhiteSpace(extra))
        {
            var trimmed = extra.Trim().Trim('"');
            if (Directory.Exists(trimmed))
            {
                yield return trimmed;
            }
        }

        var baseDir = AppContext.BaseDirectory;
        yield return Path.Combine(baseDir, "Assets");
        yield return Path.Combine(baseDir, "Assets", "Tools");
    }

    public static string? FindFile(string fileName)
    {
        foreach (var root in SearchRoots())
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            var direct = Path.Combine(root, fileName);
            if (File.Exists(direct))
            {
                return direct;
            }
        }

        return null;
    }

    public static string? ResolveDefenderRemover() => FindFile("DefenderRemover.exe");

    public static string? ResolveAdvancedRun()
    {
        foreach (var root in SearchRoots())
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var rel in new[]
                     {
                         Path.Combine("advancedrun-x64", "AdvancedRun.exe"),
                         Path.Combine("AdvancedRun", "AdvancedRun.exe"),
                         "AdvancedRun.exe",
                     })
            {
                var p = Path.Combine(root, rel);
                if (File.Exists(p))
                {
                    return p;
                }
            }
        }

        return null;
    }

    /// <summary>ExecTI / RunAsTI style launchers (folder or single file).</summary>
    public static string? ResolveExecTiLauncher()
    {
        foreach (var root in SearchRoots())
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var name in new[] { "ExecTI.cmd", "RunAsTI.cmd", "ExecTI.exe" })
            {
                var inRoot = Path.Combine(root, name);
                if (File.Exists(inRoot))
                {
                    return inRoot;
                }

                var inExecTi = Path.Combine(root, "ExecTI", name);
                if (File.Exists(inExecTi))
                {
                    return inExecTi;
                }
            }
        }

        return null;
    }

    public static string? ResolveWebView2Bootstrapper() => FindFile("MicrosoftEdgeWebview2Setup.exe");

    public static string? ResolveVcRedistAio() => FindFile("VisualCppRedist_AIO_x86_x64.exe");

    public static string? ResolveDirectXWebSetup() => FindFile("dxwebsetup.exe");

    public static string? ResolveDesktopAppInstallerBundle()
    {
        foreach (var root in SearchRoots())
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            try
            {
                var hit = Directory
                    .EnumerateFiles(root, "Microsoft.DesktopAppInstaller*.msixbundle", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (hit is not null)
                {
                    return hit;
                }
            }
            catch
            {
                // ignore
            }
        }

        return null;
    }

    public static string SummarizeFoundTools()
    {
        var parts = new List<string>();
        void Add(string label, string? path)
        {
            if (path is not null)
            {
                parts.Add($"{label}: {Path.GetFileName(path)}");
            }
        }

        Add("DefenderRemover", ResolveDefenderRemover());
        Add("AdvancedRun", ResolveAdvancedRun());
        Add("ExecTI", ResolveExecTiLauncher());
        Add("WebView2", ResolveWebView2Bootstrapper());
        Add("VC++ AIO", ResolveVcRedistAio());
        Add("DirectX web", ResolveDirectXWebSetup());
        Add("App Installer", ResolveDesktopAppInstallerBundle());
        return parts.Count == 0
            ? "No bundles under Assets (add tools to Assets or Assets\\Tools and rebuild)."
            : string.Join(" · ", parts);
    }
}

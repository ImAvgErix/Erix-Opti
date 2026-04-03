using System.Diagnostics;
using System.Runtime.InteropServices;
using ErixOpti.Core.Services;
using Microsoft.Extensions.Hosting;

namespace ErixOpti;

/// <summary>
/// Raises foreground game processes and lowers known bloat when optimization has completed.
/// </summary>
public sealed class GameExecutionerHostedService(IOptimizeSessionState session) : BackgroundService
{
    private static readonly HashSet<string> BloatProcesses =
    [
        "onedrive", "teams", "ms-teams", "outlook", "searchhost", "searchapp",
    ];

    private static readonly string[] GamePathHints =
    [
        "steamapps", "epic games", "riot games", "battle.net", "ubisoft", "ea games", "call of duty",
        "fortnite", "valorant", "cs2", "csgo", "minecraft", "elden ring",
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (session.ExecutionerActive)
                {
                    TickOnce();
                }
            }
            catch
            {
                // never take down the host
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static void TickOnce()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        _ = GetWindowThreadProcessId(hwnd, out var pid);
        if (pid == 0)
        {
            return;
        }

        try
        {
            using var p = Process.GetProcessById((int)pid);
            var path = SafeMainModulePath(p);
            var name = p.ProcessName.ToLowerInvariant();

            var h = OpenProcess(ProcessAccess, false, (int)pid);
            if (h == IntPtr.Zero)
            {
                return;
            }

            try
            {
                if (LooksLikeGame(path))
                {
                    Boost(h);
                    return;
                }

                if (BloatProcesses.Contains(name))
                {
                    Lower(h);
                }
            }
            finally
            {
                _ = CloseHandle(h);
            }
        }
        catch
        {
            // ignore access errors for protected processes
        }
    }

    private static string? SafeMainModulePath(Process p)
    {
        try
        {
            return p.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private static bool LooksLikeGame(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        var lower = path.ToLowerInvariant();
        return GamePathHints.Any(h => lower.Contains(h, StringComparison.Ordinal));
    }

    private static void Boost(IntPtr processHandle)
    {
        if (processHandle == IntPtr.Zero)
        {
            return;
        }

        if (!SetPriorityClass(processHandle, RealtimePriorityClass))
        {
            _ = SetPriorityClass(processHandle, HighPriorityClass)
                || SetPriorityClass(processHandle, AboveNormalPriorityClass);
        }
    }

    private static void Lower(IntPtr processHandle)
    {
        if (processHandle == IntPtr.Zero)
        {
            return;
        }

        _ = SetPriorityClass(processHandle, BelowNormalPriorityClass);
    }

    private const uint ProcessAccess = 0x1000 | 0x0200;

    private const uint RealtimePriorityClass = 0x100;
    private const uint HighPriorityClass = 0x80;
    private const uint AboveNormalPriorityClass = 0x8000;
    private const uint BelowNormalPriorityClass = 0x4000;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);
}

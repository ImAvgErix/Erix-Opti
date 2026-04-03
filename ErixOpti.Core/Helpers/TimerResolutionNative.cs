using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ErixOpti.Core.Helpers;

public static class TimerResolutionNative
{
    public const uint StatusSuccess = 0;

    [DllImport("ntdll.dll")]
    private static extern uint NtQueryTimerResolution(out uint min, out uint max, out uint current);

    [DllImport("ntdll.dll")]
    private static extern uint NtSetTimerResolution(uint desiredResolution, bool setResolution, out uint currentResolution);

    [DllImport("kernel32.dll")]
    private static extern void Sleep(int milliseconds);

    public static bool Query(out double minMs, out double maxMs, out double currentMs)
    {
        minMs = maxMs = currentMs = 0;
        var s = NtQueryTimerResolution(out var min, out var max, out var cur);
        if (s != StatusSuccess)
        {
            return false;
        }

        minMs = min / 10_000.0;
        maxMs = max / 10_000.0;
        currentMs = cur / 10_000.0;
        return true;
    }

    public static bool SetMaximum(out double appliedMs)
    {
        appliedMs = 0;
        var s = NtQueryTimerResolution(out var minU, out _, out _);
        if (s != StatusSuccess)
        {
            return false;
        }

        s = NtSetTimerResolution(minU, true, out var cur);
        if (s != StatusSuccess)
        {
            return false;
        }

        appliedMs = cur / 10_000.0;
        return true;
    }

    /// <summary>Coarse measurement of effective sleep granularity using QPC (similar spirit to TimerResolution tooling).</summary>
    public static double MeasureSleepGranularityMs(int samples = 96)
    {
        if (samples < 8)
        {
            samples = 8;
        }

        var deltas = new double[samples];
        for (var i = 0; i < samples; i++)
        {
            var t0 = Stopwatch.GetTimestamp();
            Sleep(1);
            var t1 = Stopwatch.GetTimestamp();
            deltas[i] = (t1 - t0) * 1000.0 / Stopwatch.Frequency;
        }
        Array.Sort(deltas);
        return deltas[samples / 2];
    }

    public static void RunKeeperLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            _ = SetMaximum(out _);
            try
            {
                Task.Delay(TimeSpan.FromSeconds(30), ct).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}

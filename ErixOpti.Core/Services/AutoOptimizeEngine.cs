using ErixOpti.Core.Helpers;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services;

public interface IAutoOptimizeEngine
{
    Task<OptimizePlan> BuildPlanAsync(CancellationToken ct);
    Task ExecutePlanAsync(OptimizePlan plan, IProgress<OptimizeProgress> progress, CancellationToken ct);
}

public sealed class AutoOptimizeEngine(
    IHardwareService hw,
    IOptimizeSessionState session) : IAutoOptimizeEngine
{
    private readonly IHardwareService _hw = hw;
    private readonly IOptimizeSessionState _session = session;

    public Task<OptimizePlan> BuildPlanAsync(CancellationToken ct)
    {
        _ = ct;
        var info = _hw.Current;
        var planned = HardwareDecisionEngine.BuildPlan(info);
        return Task.FromResult(new OptimizePlan
        {
            Operations = [.. planned.Select(p => p.Operation)],
            Planned = planned,
        });
    }

    public async Task ExecutePlanAsync(OptimizePlan plan, IProgress<OptimizeProgress> progress, CancellationToken ct)
    {
        _session.ClearLastRun();
        _session.ResetSessionFlags();

        OptimizationFileLog.Write($"── Optimize start: {plan.TotalCount} operations ──");

        int succeeded = 0, failed = 0;
        for (var i = 0; i < plan.TotalCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var op = plan.Operations[i];
            var reason = i < plan.Planned.Count ? plan.Planned[i].DecisionReason : "";

            progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, $"Applying {op.Name}..."));
            OptimizationFileLog.Write($"[{i + 1}/{plan.TotalCount}] BEGIN {op.Id} — {op.Name}");

            try
            {
                var sp = new Progress<string>(msg =>
                {
                    OptimizationFileLog.Write($"    {msg}");
                    progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, msg));
                });
                await op.Apply(sp, ct).ConfigureAwait(false);
                succeeded++;
                _session.AddStep(op.Name, reason, ok: true);
                OptimizationFileLog.Write($"[{i + 1}/{plan.TotalCount}] OK {op.Id}");
                if (op.Id == "post.timer-resolution")
                {
                    TimerResolutionNative.Query(out _, out _, out var cur);
                    _session.MeasuredTimerResolutionMs = $"{cur:0.###} ms (Nt current)";
                }
            }
            catch (Exception ex)
            {
                failed++;
                _session.AddStep(op.Name, reason, ok: false);
                OptimizationFileLog.Write($"[{i + 1}/{plan.TotalCount}] FAIL {op.Id}: {ex}");
                progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, $"Failed: {ex.Message}"));
            }
        }

        var pending = RebootDetectionHelper.IsRebootPending();
        var majorProfile = plan.Operations.Any(o =>
            string.Equals(o.Category, "Security", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(o.Category, "Post", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(o.Category, "Apps", StringComparison.OrdinalIgnoreCase));
        _session.SetRebootRecommended(pending || majorProfile);

        _session.ExecutionerActive = true;

        var summary = failed == 0
            ? $"Applied {succeeded} optimizations successfully."
            : $"Completed: {succeeded} succeeded, {failed} failed out of {plan.TotalCount}.";
        OptimizationFileLog.Write($"── Optimize end — {summary} RebootPending={pending} MajorProfile={majorProfile} ──");
        progress.Report(new OptimizeProgress(plan.TotalCount, plan.TotalCount, "Done", summary));
    }
}

using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services;

public interface IAutoOptimizeEngine
{
    Task<OptimizePlan> BuildPlanAsync(CancellationToken ct);
    Task ExecutePlanAsync(OptimizePlan plan, IProgress<OptimizeProgress> progress, CancellationToken ct);
}

public sealed class AutoOptimizeEngine : IAutoOptimizeEngine
{
    private readonly IHardwareService _hw;
    public AutoOptimizeEngine(IHardwareService hw) => _hw = hw;

    public Task<OptimizePlan> BuildPlanAsync(CancellationToken ct)
    {
        var info = _hw.Current;
        var all = TweakCatalog.All(info);
        return Task.FromResult(new OptimizePlan { Operations = all.Where(t => t.ShouldApply(info)).ToList() });
    }

    public async Task ExecutePlanAsync(OptimizePlan plan, IProgress<OptimizeProgress> progress, CancellationToken ct)
    {
        int succeeded = 0, failed = 0;
        for (int i = 0; i < plan.TotalCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var op = plan.Operations[i];
            progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, $"Applying {op.Name}..."));
            try
            {
                var sp = new Progress<string>(msg => progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, msg)));
                await op.Apply(sp, ct).ConfigureAwait(false);
                succeeded++;
            }
            catch (Exception ex)
            {
                failed++;
                progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, $"Failed: {ex.Message}"));
            }
        }
        var summary = failed == 0
            ? $"Applied {succeeded} optimizations successfully."
            : $"Completed: {succeeded} succeeded, {failed} failed out of {plan.TotalCount}.";
        progress.Report(new OptimizeProgress(plan.TotalCount, plan.TotalCount, "Done", summary));
    }
}

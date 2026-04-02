using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services.Tweaks;

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
        HwRef.Hw = info;
        var all = new List<TweakOperation>();
        all.AddRange(RegistryTweaks.All);
        all.AddRange(ServiceTweaks.All);
        all.AddRange(PowerTweaks.All);
        all.AddRange(GpuTweaks.All);
        all.AddRange(NetworkTweaks.All);
        all.AddRange(CleanupTweaks.All);
        return Task.FromResult(new OptimizePlan { Operations = all.Where(t => t.ShouldApply(info)).ToList() });
    }

    public async Task ExecutePlanAsync(OptimizePlan plan, IProgress<OptimizeProgress> progress, CancellationToken ct)
    {
        for (int i = 0; i < plan.TotalCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var op = plan.Operations[i];
            progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, $"Applying {op.Name}..."));
            try
            {
                var sp = new Progress<string>(msg => progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, msg)));
                await op.Apply(sp, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                progress.Report(new OptimizeProgress(i + 1, plan.TotalCount, op.Name, $"Failed: {ex.Message}"));
            }
        }
        progress.Report(new OptimizeProgress(plan.TotalCount, plan.TotalCount, "Done", $"Applied {plan.TotalCount} optimizations."));
    }
}

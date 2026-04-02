namespace ErixOpti.Core.Models;

public sealed class OptimizePlan
{
    public IReadOnlyList<TweakOperation> Operations { get; init; } = [];
    public int TotalCount => Operations.Count;
}

public sealed record OptimizeProgress(int Current, int Total, string StepName, string Detail);

namespace ErixOpti.Core.Models;

public sealed class TweakOperation
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public string Description { get; init; } = "";

    /// <summary>Lower runs first within the same category band.</summary>
    public int PlanOrder { get; init; }

    public required Func<HardwareInfo, bool> ShouldApply { get; init; }
    public required Func<IProgress<string>, CancellationToken, Task> Apply { get; init; }
    public required Func<IProgress<string>, CancellationToken, Task> Revert { get; init; }

    public Func<HardwareInfo, string>? ExplainDecision { get; init; }

    /// <summary>True = tweak already matches optimized state, false = not, null = cannot determine.</summary>
    public Func<HardwareInfo, bool?>? TryGetAppliedState { get; init; }

    /// <summary>Optional async probe. When non-null, status UI prefers this over <see cref="TryGetAppliedState"/>.</summary>
    public Func<HardwareInfo, CancellationToken, Task<bool?>>? TryGetAppliedStateAsync { get; init; }
}

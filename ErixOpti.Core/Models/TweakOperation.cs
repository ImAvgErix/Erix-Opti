namespace ErixOpti.Core.Models;

public sealed class TweakOperation
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required Func<HardwareInfo, bool> ShouldApply { get; init; }
    public required Func<IProgress<string>, CancellationToken, Task> Apply { get; init; }
    public required Func<IProgress<string>, CancellationToken, Task> Revert { get; init; }
}

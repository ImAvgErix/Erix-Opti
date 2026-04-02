using ErixOpti.Core.Models;

namespace ErixOpti.Core.Interfaces;

public interface ITweak
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Category { get; }
    RiskLevel Risk { get; }
    bool RequiresReboot { get; }

    Task ApplyAsync(IProgress<string> progress, CancellationToken ct);

    Task RevertAsync(IProgress<string> progress, CancellationToken ct);

    Task<bool> IsApplicableAsync(CancellationToken ct);

    Task<bool> IsAppliedAsync(CancellationToken ct);
}

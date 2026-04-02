using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services.Tweaks;

public abstract class TweakBase : ITweak
{
    protected TweakBase(IHardwareService hardware)
    {
        Hardware = hardware;
    }

    protected IHardwareService Hardware { get; }

    public abstract string Id { get; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string Category { get; }

    public abstract RiskLevel Risk { get; }

    public abstract bool RequiresReboot { get; }

    public abstract Task ApplyAsync(IProgress<string> progress, CancellationToken ct);

    public abstract Task RevertAsync(IProgress<string> progress, CancellationToken ct);

    public abstract Task<bool> IsApplicableAsync(CancellationToken ct);

    public abstract Task<bool> IsAppliedAsync(CancellationToken ct);
}

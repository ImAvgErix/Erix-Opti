namespace ErixOpti.Core.Services;

public sealed class OptimizeSessionState : IOptimizeSessionState
{
    private readonly object _gate = new();
    private readonly List<OptimizeStepRecord> _steps = [];

    public bool RebootRecommended { get; private set; }

    public IReadOnlyList<OptimizeStepRecord> LastRunSteps
    {
        get
        {
            lock (_gate)
            {
                return [.. _steps];
            }
        }
    }

    public void ClearLastRun()
    {
        lock (_gate)
        {
            _steps.Clear();
        }
    }

    public void AddStep(string name, string reason, bool ok)
    {
        lock (_gate)
        {
            _steps.Add(new OptimizeStepRecord(name, reason, ok));
        }
    }

    public void SetRebootRecommended(bool value) => RebootRecommended = value;

    public void ResetSessionFlags() => RebootRecommended = false;
}

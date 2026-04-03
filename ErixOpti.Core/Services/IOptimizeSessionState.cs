namespace ErixOpti.Core.Services;

public interface IOptimizeSessionState
{
    bool RebootRecommended { get; }

    string? MeasuredTimerResolutionMs { get; set; }

    bool ExecutionerActive { get; set; }

    IReadOnlyList<OptimizeStepRecord> LastRunSteps { get; }

    void ClearLastRun();

    void AddStep(string name, string reason, bool ok);

    void SetRebootRecommended(bool value);

    void ResetSessionFlags();
}

public sealed record OptimizeStepRecord(string Name, string Reason, bool Ok);

using System.Collections.ObjectModel;

namespace ErixOpti.Core.ViewModels;

public enum TweakUiStatus
{
    Active,
    Inactive,
    Unknown,
    Skipped,
    OneShot,
}

public sealed class TweakCategoryVm
{
    public TweakCategoryVm(string name) => Name = name;
    public string Name { get; }
    public ObservableCollection<TweakRowVm> Items { get; } = new();
}

public sealed class PlannedDecisionRowVm(string title, string reason)
{
    public string Title { get; } = title;
    public string Reason { get; } = reason;
}

public sealed class LastOptimizeStepVm(string name, string reason, bool ok)
{
    public string Name { get; } = name;
    public string Reason { get; } = reason;
    public bool Ok { get; } = ok;
    public string StatusLabel => Ok ? "Applied" : "Failed";
}

public sealed class TweakRowVm
{
    public TweakRowVm(string id, string displayName, TweakUiStatus status, string statusLabel, string accentHex)
    {
        Id = id;
        DisplayName = displayName;
        Status = status;
        StatusLabel = statusLabel;
        AccentHex = accentHex;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public TweakUiStatus Status { get; }
    public string StatusLabel { get; }
    public string AccentHex { get; }
}

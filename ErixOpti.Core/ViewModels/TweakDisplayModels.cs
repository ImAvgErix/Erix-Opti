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

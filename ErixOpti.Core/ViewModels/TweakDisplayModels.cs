using System.Collections.ObjectModel;

namespace ErixOpti.Core.ViewModels;

public enum TweakUiStatus { Active, Inactive, Unknown, Skipped, OneShot }

public sealed class TweakCategoryVm(string name)
{
    public string Name { get; } = name;
    public ObservableCollection<TweakRowVm> Items { get; } = [];
}

public sealed class TweakRowVm(string id, string displayName, string description, TweakUiStatus status, string statusLabel, string accentHex)
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
    public TweakUiStatus Status { get; } = status;
    public string StatusLabel { get; } = statusLabel;
    public string AccentHex { get; } = accentHex;
}

using System.Collections.ObjectModel;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services;

namespace ErixOpti.Core.ViewModels;

public static class TweakListBuilder
{
    public static readonly string[] CategoryOrder =
    [
        "Input", "System", "Memory", "Gaming", "GPU", "Power", "Network",
        "Privacy", "AI Removal", "Appearance", "Explorer", "Visual", "Storage",
        "Device", "Services", "Cleanup",
    ];

    public static async Task RebuildAsync(
        ObservableCollection<TweakCategoryVm> target, HardwareInfo hw, CancellationToken ct)
    {
        var ops = TweakCatalog.All(hw);
        var map = ops.GroupBy(o => o.Category)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());
        target.Clear();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        async Task AddCat(string cat, List<TweakOperation> list)
        {
            var vm = new TweakCategoryVm(cat);
            foreach (var op in list) vm.Items.Add(await MakeRowAsync(op, hw, ct));
            target.Add(vm);
        }

        foreach (var cat in CategoryOrder)
        {
            if (!map.TryGetValue(cat, out var list)) continue;
            seen.Add(cat);
            await AddCat(cat, list);
        }
        foreach (var cat in map.Keys.Where(k => !seen.Contains(k)).OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            await AddCat(cat, map[cat]);
    }

    public static async Task<(int Active, int Eligible, int Total)> CountAsync(HardwareInfo hw, CancellationToken ct)
    {
        var ops = TweakCatalog.All(hw);
        int active = 0, eligible = 0, total = ops.Count;
        foreach (var op in ops)
        {
            if (!op.ShouldApply(hw) || op.Id.StartsWith("clean.", StringComparison.Ordinal)) continue;
            eligible++;
            var probe = op.TryGetAppliedStateAsync is not null
                ? await op.TryGetAppliedStateAsync(hw, ct)
                : op.TryGetAppliedState?.Invoke(hw);
            if (probe is true) active++;
        }
        return (active, eligible, total);
    }

    private static async Task<TweakRowVm> MakeRowAsync(TweakOperation op, HardwareInfo hw, CancellationToken ct)
    {
        var desc = string.IsNullOrWhiteSpace(op.Description) ? op.Id : op.Description;
        if (!op.ShouldApply(hw))
            return new TweakRowVm(op.Id, op.Name, desc, TweakUiStatus.Skipped, "Skipped", "#78716C");
        if (op.Id.StartsWith("clean.", StringComparison.Ordinal))
            return new TweakRowVm(op.Id, op.Name, desc, TweakUiStatus.OneShot, "One-shot", "#C084FC");

        var r = op.TryGetAppliedStateAsync is not null
            ? await op.TryGetAppliedStateAsync(hw, ct)
            : op.TryGetAppliedState?.Invoke(hw);

        return r switch
        {
            null => new TweakRowVm(op.Id, op.Name, desc, TweakUiStatus.Unknown, "Unknown", "#FBBF24"),
            true => new TweakRowVm(op.Id, op.Name, desc, TweakUiStatus.Active, "Active", "#34D399"),
            false => new TweakRowVm(op.Id, op.Name, desc, TweakUiStatus.Inactive, "Pending", "#94A3B8"),
        };
    }
}

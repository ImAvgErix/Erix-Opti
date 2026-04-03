using System.Collections.ObjectModel;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services;

namespace ErixOpti.Core.ViewModels;

public static class TweakListBuilder
{
    public static readonly string[] CategoryOrder =
    [
        "Input", "System", "Memory", "Gaming", "Privacy", "Explorer", "Visual", "Storage",
        "GPU", "Power", "Network", "Services", "Cleanup",
    ];

    public static void Rebuild(ObservableCollection<TweakCategoryVm> target, HardwareInfo hw)
    {
        var ops = TweakCatalog.All(hw);
        var map = ops.GroupBy(o => o.Category).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());
        target.Clear();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var cat in CategoryOrder)
        {
            if (!map.TryGetValue(cat, out var list))
                continue;
            seen.Add(cat);
            var vm = new TweakCategoryVm(cat);
            foreach (var op in list)
                vm.Items.Add(MakeRow(op, hw));
            target.Add(vm);
        }

        foreach (var cat in map.Keys.Where(k => !seen.Contains(k)).OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
        {
            var vm = new TweakCategoryVm(cat);
            foreach (var op in map[cat])
                vm.Items.Add(MakeRow(op, hw));
            target.Add(vm);
        }
    }

    public static (int Active, int EligibleProbeable, int Total) CountSummary(HardwareInfo hw)
    {
        var ops = TweakCatalog.All(hw);
        int active = 0, eligible = 0, total = ops.Count;
        foreach (var op in ops)
        {
            if (!op.ShouldApply(hw))
                continue;
            if (op.Id.StartsWith("clean.", StringComparison.Ordinal))
                continue;
            eligible++;
            var probe = op.TryGetAppliedState;
            if (probe?.Invoke(hw) is true)
                active++;
        }

        return (active, eligible, total);
    }

    private static TweakRowVm MakeRow(TweakOperation op, HardwareInfo hw)
    {
        if (!op.ShouldApply(hw))
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.Skipped, "Skipped on this PC", "#78716C");

        if (op.Id.StartsWith("clean.", StringComparison.Ordinal))
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.OneShot, "Runs when you optimize", "#C084FC");

        var probe = op.TryGetAppliedState;
        if (probe is null)
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.Unknown, "Status not detected", "#FBBF24");

        var r = probe(hw);
        if (r is null)
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.Unknown, "Status not detected", "#FBBF24");

        return r.Value
            ? new TweakRowVm(op.Id, op.Name, TweakUiStatus.Active, "Already on", "#34D399")
            : new TweakRowVm(op.Id, op.Name, TweakUiStatus.Inactive, "Not applied yet", "#94A3B8");
    }
}

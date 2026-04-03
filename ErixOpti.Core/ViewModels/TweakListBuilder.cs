using System.Collections.ObjectModel;
using ErixOpti.Core.Models;
using ErixOpti.Core.Services;

namespace ErixOpti.Core.ViewModels;

public static class TweakListBuilder
{
    public static readonly string[] CategoryOrder =
    [
        "Input", "System", "Memory", "Gaming", "GPU", "Power", "Network",
        "Privacy", "Security", "Explorer", "Visual", "Storage",
        "Services", "Apps", "Tasks", "Cleanup", "Post",
    ];

    public static async Task RebuildAsync(
        ObservableCollection<TweakCategoryVm> target,
        HardwareInfo hw,
        CancellationToken ct)
    {
        var ops = TweakCatalog.All(hw);
        var map = ops.GroupBy(o => o.Category).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());
        target.Clear();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        async Task AddCategoryAsync(string cat, List<TweakOperation> list)
        {
            var vm = new TweakCategoryVm(cat);
            foreach (var op in list)
            {
                vm.Items.Add(await MakeRowAsync(op, hw, ct).ConfigureAwait(false));
            }

            target.Add(vm);
        }

        foreach (var cat in CategoryOrder)
        {
            if (!map.TryGetValue(cat, out var list))
            {
                continue;
            }

            seen.Add(cat);
            await AddCategoryAsync(cat, list).ConfigureAwait(false);
        }

        foreach (var cat in map.Keys.Where(k => !seen.Contains(k)).OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
        {
            await AddCategoryAsync(cat, map[cat]).ConfigureAwait(false);
        }
    }

    public static async Task<(int Active, int EligibleProbeable, int Total)> CountSummaryAsync(HardwareInfo hw, CancellationToken ct)
    {
        var ops = TweakCatalog.All(hw);
        int active = 0, eligible = 0, total = ops.Count;
        foreach (var op in ops)
        {
            if (!op.ShouldApply(hw))
            {
                continue;
            }

            if (op.Id.StartsWith("clean.", StringComparison.Ordinal) || op.Category == "Post")
            {
                continue;
            }

            eligible++;
            bool? probe;
            if (op.TryGetAppliedStateAsync is not null)
            {
                probe = await op.TryGetAppliedStateAsync(hw, ct).ConfigureAwait(false);
            }
            else
            {
                probe = op.TryGetAppliedState?.Invoke(hw);
            }

            if (probe is true)
            {
                active++;
            }
        }

        return (active, eligible, total);
    }

    private static async Task<TweakRowVm> MakeRowAsync(TweakOperation op, HardwareInfo hw, CancellationToken ct)
    {
        if (!op.ShouldApply(hw))
        {
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.Skipped, "Skipped on this PC", "#78716C");
        }

        if (op.Id.StartsWith("clean.", StringComparison.Ordinal) || op.Category == "Post")
        {
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.OneShot, "Runs during Auto Optimize", "#C084FC");
        }

        bool? r;
        if (op.TryGetAppliedStateAsync is not null)
        {
            r = await op.TryGetAppliedStateAsync(hw, ct).ConfigureAwait(false);
        }
        else
        {
            r = op.TryGetAppliedState?.Invoke(hw);
        }

        if (r is null)
        {
            return new TweakRowVm(op.Id, op.Name, TweakUiStatus.Unknown, "Status not detected", "#FBBF24");
        }

        return r.Value
            ? new TweakRowVm(op.Id, op.Name, TweakUiStatus.Active, "Already on", "#34D399")
            : new TweakRowVm(op.Id, op.Name, TweakUiStatus.Inactive, "Not applied yet", "#94A3B8");
    }
}

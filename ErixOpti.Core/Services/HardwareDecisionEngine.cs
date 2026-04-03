using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services;

public static class HardwareDecisionEngine
{
    private static int CategoryBand(string category) => category switch
    {
        "Input" => 0,
        "System" => 10,
        "Memory" => 20,
        "Gaming" => 30,
        "GPU" => 35,
        "Power" => 40,
        "Network" => 45,
        "Privacy" => 50,
        "AI Removal" => 55,
        "Dark Mode" => 58,
        "Explorer" => 60,
        "Visual" => 65,
        "Storage" => 70,
        "Services" => 80,
        "Cleanup" => 900,
        _ => 100,
    };

    public static IReadOnlyList<PlannedTweak> BuildPlan(HardwareInfo hw)
    {
        var raw = TweakCatalog.All(hw).Where(t => t.ShouldApply(hw)).ToList();
        raw.Sort((a, b) =>
        {
            var ba = CategoryBand(a.Category);
            var bb = CategoryBand(b.Category);
            var c = ba.CompareTo(bb);
            if (c != 0) return c;
            c = a.PlanOrder.CompareTo(b.PlanOrder);
            return c != 0 ? c : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });

        return raw.Select(t => new PlannedTweak(t, t.ExplainDecision?.Invoke(hw) ?? DefaultReason(t, hw))).ToList();
    }

    private static string DefaultReason(TweakOperation op, HardwareInfo hw) =>
        $"Selected for {hw.FormFactor} ({hw.CpuManufacturer} CPU, {hw.PrimaryGpuVendor} GPU, {hw.RamTotalGb:0.#} GB RAM).";
}

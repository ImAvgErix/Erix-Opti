using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services;

public static class HardwareDecisionEngine
{
    private static int Band(string cat) => cat switch
    {
        "Input" => 0, "System" => 10, "Memory" => 20, "Gaming" => 30, "GPU" => 35,
        "Power" => 40, "Network" => 45, "Privacy" => 50, "AI Removal" => 55,
        "Appearance" => 58, "Explorer" => 60, "Visual" => 65, "Storage" => 70,
        "Device" => 75, "Services" => 80, "Cleanup" => 900, _ => 100,
    };

    public static IReadOnlyList<PlannedTweak> BuildPlan(HardwareInfo hw)
    {
        var raw = TweakCatalog.All(hw).Where(t => t.ShouldApply(hw)).ToList();
        raw.Sort((a, b) =>
        {
            var c = Band(a.Category).CompareTo(Band(b.Category));
            if (c != 0) return c;
            c = a.PlanOrder.CompareTo(b.PlanOrder);
            return c != 0 ? c : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });
        return raw.Select(t => new PlannedTweak(t, t.ExplainDecision?.Invoke(hw) ?? $"Selected for {hw.FormFactor} ({hw.CpuManufacturer}, {hw.PrimaryGpuVendor}, {hw.RamTotalGb:0.#} GB).")).ToList();
    }
}

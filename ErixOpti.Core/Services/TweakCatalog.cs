using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Services.Tweaks;

namespace ErixOpti.Core.Services;

public sealed class TweakCatalog : ITweakCatalog
{
    private readonly IReadOnlyList<ITweak> _all;

    public TweakCatalog(IHardwareService hardware)
    {
        _all =
        [
            new HagsTweak(hardware),
            new GameModeTweak(hardware),
            new UltimatePerformanceTweak(hardware),
            new ReBarTweak(hardware),
            new SystemResponsivenessTweak(hardware),
            new CoreParkingTweak(hardware),
            new OptionalServicesTweak(hardware),
            new TelemetryTweak(hardware),
            new PrivacyExtrasTweak(hardware),
            new VisualEffectsTweak(hardware),
            new StorageSenseTweak(hardware),
            new PrefetchSsdTweak(hardware),
            new HvciTweak(hardware),
            new BcdBootTweak(hardware),
            new SpectreMitigationTweak(hardware)
        ];
    }

    public IReadOnlyList<ITweak> All => _all;

    public ITweak? GetById(string id) => _all.FirstOrDefault(t => t.Id == id);

    public IReadOnlyList<ITweak> GetByPreset(TweakPreset preset)
    {
        var ids = preset switch
        {
            TweakPreset.Gaming => new[]
            {
                "perf.hags", "perf.gamemode", "perf.ultimate-power", "perf.rebar", "perf.system-responsiveness",
                "perf.core-parking", "storage.prefetch-ssd", "visual.effects"
            },
            TweakPreset.Privacy => new[] { "privacy.telemetry", "privacy.ads-activity", "svc.optional-disable" },
            TweakPreset.Balanced => new[] { "perf.gamemode", "privacy.telemetry", "visual.effects" },
            TweakPreset.Extreme => new[]
            {
                "perf.hags", "perf.ultimate-power", "perf.core-parking", "storage.prefetch-ssd", "svc.optional-disable",
                "advanced.hvci-disable", "advanced.bcd-boot", "advanced.spectre-mitigations"
            },
            _ => Array.Empty<string>()
        };

        return ids.Select(id => GetById(id)).Where(t => t is not null).Cast<ITweak>().ToArray();
    }
}

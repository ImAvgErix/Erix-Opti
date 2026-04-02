namespace ErixOpti.Core.Interfaces;

public interface ITweakCatalog
{
    IReadOnlyList<ITweak> All { get; }

    IReadOnlyList<ITweak> GetByPreset(TweakPreset preset);

    ITweak? GetById(string id);
}

public enum TweakPreset
{
    Gaming,
    Privacy,
    Balanced,
    Extreme
}

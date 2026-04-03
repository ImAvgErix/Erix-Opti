using ErixOpti.Core.Models;

namespace ErixOpti.Core.Interfaces;

public interface IHardwareService
{
    HardwareInfo Current { get; }

    event EventHandler? HardwareUpdated;

    Task StartAsync(CancellationToken ct = default);

    Task RefreshAsync(CancellationToken ct = default);

    Task StopAsync();
}

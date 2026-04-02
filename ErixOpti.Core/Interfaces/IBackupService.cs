namespace ErixOpti.Core.Interfaces;

public interface IBackupService
{
    string BackupRoot { get; }

    Task<BackupResult> CreateFullBackupAsync(IProgress<string> progress, CancellationToken ct);
}

public sealed record BackupResult(bool Success, string? RestorePointId, string? RegistryPath, string? BcdPath, string? Error);

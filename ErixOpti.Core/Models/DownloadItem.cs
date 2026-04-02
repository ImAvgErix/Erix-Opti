namespace ErixOpti.Core.Models;

public sealed class DownloadItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required string Url { get; init; }
    public string? FileName { get; init; }
    public string? SilentArgs { get; init; }
    public bool IsDirectDownload { get; init; } = true;
}

public sealed record DownloadProgress(long BytesReceived, long TotalBytes, double SpeedMBps);

public enum DownloadState { Ready, Downloading, Downloaded, Installing, Installed, Failed }

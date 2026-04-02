using System.Diagnostics;
using System.Net.Http;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.Services;

public interface IDownloadManager
{
    Task<string> DownloadAsync(DownloadItem item, IProgress<DownloadProgress> progress, CancellationToken ct);
    Task InstallAsync(string filePath, string? silentArgs, CancellationToken ct);
}

public sealed class DownloadManager : IDownloadManager
{
    private static readonly HttpClient Http = new();
    private readonly string _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ErixOpti", "Downloads");

    public DownloadManager() => Directory.CreateDirectory(_root);

    public async Task<string> DownloadAsync(DownloadItem item, IProgress<DownloadProgress> progress, CancellationToken ct)
    {
        var fn = item.FileName ?? Path.GetFileName(new Uri(item.Url).AbsolutePath);
        if (string.IsNullOrWhiteSpace(fn)) fn = $"{item.Id}.exe";
        var dest = Path.Combine(_root, fn);
        using var resp = await Http.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength ?? -1L;
        await using var cs = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await using var fs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        var buf = new byte[81920]; long rx = 0; var sw = Stopwatch.StartNew(); int read;
        while ((read = await cs.ReadAsync(buf, ct).ConfigureAwait(false)) > 0)
        {
            await fs.WriteAsync(buf.AsMemory(0, read), ct).ConfigureAwait(false);
            rx += read;
            var spd = sw.Elapsed.TotalSeconds > 0 ? rx / 1024.0 / 1024.0 / sw.Elapsed.TotalSeconds : 0;
            progress.Report(new DownloadProgress(rx, total, spd));
        }
        return dest;
    }

    public async Task InstallAsync(string filePath, string? silentArgs, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(filePath, silentArgs ?? "/quiet /norestart") { UseShellExecute = true, Verb = "runas" };
        using var proc = Process.Start(psi);
        if (proc != null) await proc.WaitForExitAsync(ct).ConfigureAwait(false);
    }
}

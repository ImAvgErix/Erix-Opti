using ErixOpti.Core.Interfaces;

namespace ErixOpti.Core.Services;

public sealed class TweakApplyService
{
    private static readonly IProgress<string> NoopProgress = new Progress<string>(_ => { });

    public async Task ApplySafeAsync(ITweak tweak, IProgress<string> progress, CancellationToken ct)
    {
        if (!await tweak.IsApplicableAsync(ct).ConfigureAwait(false))
        {
            throw new InvalidOperationException("This tweak is not applicable on this system right now.");
        }

        try
        {
            await tweak.ApplyAsync(progress, ct).ConfigureAwait(false);
        }
        catch
        {
            progress?.Report("Apply failed — attempting automatic revert…");
            try
            {
                await tweak.RevertAsync(progress ?? NoopProgress, ct).ConfigureAwait(false);
            }
            catch
            {
                // Swallow secondary failures; original exception propagates.
            }

            throw;
        }
    }
}

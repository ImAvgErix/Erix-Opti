using ErixOpti.Core.Interfaces;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Services;

public sealed class UserDialogService : IUserDialogService
{
    private readonly WindowContext _window;

    public UserDialogService(WindowContext window)
    {
        _window = window;
    }

    public async Task<bool> ConfirmBackupBeforeTweaksAsync()
    {
        var root = _window.XamlRoot ?? throw new InvalidOperationException("Window not ready.");
        var dlg = new ContentDialog
        {
            Title = "Backup before changes?",
            Content =
                "ErixOpti will create a System Restore point, export registry hives to disk, and export your BCD store. " +
                "This is strongly recommended before applying any tweaks.",
            PrimaryButtonText = "Create backup & continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = root
        };

        return await dlg.ShowAsync() == ContentDialogResult.Primary;
    }

    public async Task<bool> ConfirmHighRiskTweakAsync(ITweak tweak)
    {
        var root = _window.XamlRoot ?? throw new InvalidOperationException("Window not ready.");
        var first = new ContentDialog
        {
            Title = "High-risk tweak (1/2)",
            Content =
                $"{tweak.Name}\n\n{tweak.Description}\n\nThis tweak is marked HIGH risk. You will be asked to confirm again.",
            PrimaryButtonText = "Continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = root
        };

        if (await first.ShowAsync() != ContentDialogResult.Primary)
        {
            return false;
        }

        var second = new ContentDialog
        {
            Title = "Confirm high-risk change (2/2)",
            Content =
                "This can reduce security or stability. Only proceed if you accept full responsibility and have backups.",
            PrimaryButtonText = "Yes — apply anyway",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = root
        };

        return await second.ShowAsync() == ContentDialogResult.Primary;
    }
}

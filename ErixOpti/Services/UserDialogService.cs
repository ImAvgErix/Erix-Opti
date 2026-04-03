using ErixOpti.Core.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ErixOpti.Services;

public sealed class UserDialogService : IUserDialogService
{
    private readonly WindowContext _wc;
    public UserDialogService(WindowContext wc) => _wc = wc;

    public async Task<bool> ConfirmBackupBeforeTweaksAsync()
    {
        var root = _wc.XamlRoot ?? throw new InvalidOperationException("Window not ready.");
        var dlg = new ContentDialog
        {
            Title = "Create backup?",
            Content = "ErixOpti will create a System Restore point, export registry, and BCD before applying changes.",
            PrimaryButtonText = "Backup & continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = root,
            CornerRadius = new CornerRadius(8),
        };
        return await dlg.ShowAsync() == ContentDialogResult.Primary;
    }

    public async Task<bool> ConfirmHighRiskAsync(string title, string message)
    {
        var root = _wc.XamlRoot ?? throw new InvalidOperationException("Window not ready.");
        var dlg = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = root,
            CornerRadius = new CornerRadius(8),
        };
        return await dlg.ShowAsync() == ContentDialogResult.Primary;
    }

    public async Task<bool> PromptRestartAfterOptimizeAsync()
    {
        var root = _wc.XamlRoot ?? throw new InvalidOperationException("Window not ready.");
        var dlg = new ContentDialog
        {
            Title = "Optimization complete",
            Content = "Optimization finished. A restart is recommended so services, packages, and policies settle cleanly. Restart now?",
            PrimaryButtonText = "Restart now",
            CloseButtonText = "Later",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = root,
            CornerRadius = new CornerRadius(8),
        };
        var r = await dlg.ShowAsync();
        return r == ContentDialogResult.Primary;
    }
}

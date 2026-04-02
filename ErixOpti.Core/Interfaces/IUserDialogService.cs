namespace ErixOpti.Core.Interfaces;

public interface IUserDialogService
{
    Task<bool> ConfirmBackupBeforeTweaksAsync();

    Task<bool> ConfirmHighRiskTweakAsync(ITweak tweak);
}

namespace ErixOpti.Core.Interfaces;

public interface IUserDialogService
{
    Task<bool> ConfirmBackupBeforeTweaksAsync();
    Task<bool> ConfirmHighRiskAsync(string title, string message);
}

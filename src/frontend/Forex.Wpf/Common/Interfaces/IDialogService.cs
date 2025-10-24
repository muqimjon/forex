namespace Forex.Wpf.Common.Interfaces;

public interface IDialogService
{
    Task<bool> ShowYesNoAsync(string message, string title = "Tasdiqlang");
    Task<bool> ShowOkCancelAsync(string message, string title = "Diqqat");
    //Task<string?> ShowInputAsync(string prompt, string title = "Ma'lumot kiriting");
    Task ShowMessageAsync(string message, string title = "Xabar");
}

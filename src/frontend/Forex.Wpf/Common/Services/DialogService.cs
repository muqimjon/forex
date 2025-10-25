namespace Forex.Wpf.Common.Services;

using Forex.Wpf.Common.Interfaces;
using System.Windows;

public class DialogService : IDialogService
{
    public Task<bool> ShowYesNoAsync(string message, string title = "Tasdiqlang")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task<bool> ShowOkCancelAsync(string message, string title = "Diqqat")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        return Task.FromResult(result == MessageBoxResult.OK);
    }

    //public Task<string?> ShowInputAsync(string prompt, string title = "Ma'lumot kiriting")
    //{
    //    var inputWindow = new InputDialogWindow(prompt, title); // custom WPF window
    //    var result = inputWindow.ShowDialog();
    //    return Task.FromResult(result == true ? inputWindow.InputText : null);
    //}

    public Task ShowMessageAsync(string message, string title = "Xabar")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }
}

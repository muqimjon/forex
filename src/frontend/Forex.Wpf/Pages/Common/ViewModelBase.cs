namespace Forex.Wpf.Pages.Common;

using Forex.Wpf.Common.Enums;
using Forex.Wpf.Common.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        if (typeof(T) == typeof(string) && !string.IsNullOrWhiteSpace(value?.ToString()))
        {
            if (propertyName is nameof(ErrorMessage))
                ShowNotification(value.ToString()!, NotificationType.Error);
            else if (propertyName is nameof(WarningMessage))
                ShowNotification(value.ToString()!, NotificationType.Warning);
            else if (propertyName is nameof(InfoMessage))
                ShowNotification(value.ToString()!, NotificationType.Info);
            else if (propertyName is nameof(SuccessMessage))
                ShowNotification(value.ToString()!, NotificationType.Success);
        }

        return true;
    }

    private static void ShowNotification(string message, NotificationType type)
    {
        // ⏳ Matn uzunligiga qarab vaqtni belgilaymiz
        // Har bir 20 ta belgiga +1 sekund, min=3s, max=10s
        int duration = Math.Clamp(3 + message.Length / 20, 3, 10);

        NotificationService.Show(
            message,
            type: type,
            durationSeconds: duration
        );
    }

    // 🔄 Loading holati
    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            if (SetProperty(ref isLoading, value))
            {
                if (value) SpinnerService.Show();
                else SpinnerService.Hide();
            }
        }
    }

    // 🔔 Notifications
    private string errorMessage = "";
    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    private string warningMessage = "";
    public string WarningMessage
    {
        get => warningMessage;
        set => SetProperty(ref warningMessage, value);
    }

    private string infoMessage = "";
    public string InfoMessage
    {
        get => infoMessage;
        set => SetProperty(ref infoMessage, value);
    }

    private string successMessage = "";
    public string SuccessMessage
    {
        get => successMessage;
        set => SetProperty(ref successMessage, value);
    }
}

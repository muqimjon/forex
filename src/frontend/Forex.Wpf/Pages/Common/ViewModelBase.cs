namespace Forex.Wpf.Pages.Common;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Common.Enums;
using Forex.Wpf.Common.Services;

public abstract partial class ViewModelBase : ObservableObject
{
    // 🔄 Spinner
    [ObservableProperty]
    private bool isLoading;

    partial void OnIsLoadingChanged(bool value)
    {
        if (value) SpinnerService.Show();
        else SpinnerService.Hide();
    }

    // 🔔 Notifications
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private string warningMessage = string.Empty;
    [ObservableProperty] private string infoMessage = string.Empty;
    [ObservableProperty] private string successMessage = string.Empty;


    partial void OnErrorMessageChanged(string value)
        => ShowNotification(value, NotificationType.Error);

    partial void OnWarningMessageChanged(string value)
        => ShowNotification(value, NotificationType.Warning);

    partial void OnInfoMessageChanged(string value)
        => ShowNotification(value, NotificationType.Info);

    partial void OnSuccessMessageChanged(string value)
        => ShowNotification(value, NotificationType.Success);

    private static void ShowNotification(string message, NotificationType type)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        int duration = Math.Clamp(3 + message.Length / 20, 3, 10);

        NotificationService.Show(message, type, durationSeconds: duration);
    }
}

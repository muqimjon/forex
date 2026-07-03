namespace Forex.Wpf.Windows.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Services;
using Forex.Wpf.Pages.Common;

public partial class ProfileEditViewModel : ViewModelBase
{
    private readonly ForexClient client;

    [ObservableProperty] private long userId;
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string? newPassword;
    [ObservableProperty] private UserRole role;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string? tmpImagePath;

    public ProfileEditViewModel(ForexClient client)
    {
        this.client = client;
        UserId = AuthStore.Instance.UserId;
        FullName = AuthStore.Instance.FullName;
        Username = AuthStore.Instance.Username;
    }

    public async Task LoadUserDataAsync()
    {
        var response = await client.Users.GetById(UserId).Handle(l => IsLoading = l);

        if (response?.Data != null)
        {
            var user = response.Data;
            FullName = user.Name;
            Username = user.Username ?? string.Empty;
            Phone = user.Phone ?? string.Empty;
            Email = user.Email ?? string.Empty;
            Address = user.Address ?? string.Empty;
            Role = user.Role;
            Description = user.Description ?? string.Empty;
        }
        else
        {
            ErrorMessage = response?.Message ?? "Foydalanuvchi ma'lumotlarini yuklashda xatolik!";
        }
    }

    public async Task<bool> SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = "Ism va familiya majburiy!";
            return false;
        }

        var request = new UserRequest
        {
            Id = UserId,
            Name = FullName,
            Username = Username,
            Phone = Phone,
            Email = Email,
            Address = Address,
            Role = Role,
            Description = Description,
            Password = NewPassword,
            Accounts = new()
        };

        // Pass tmp image path to backend
        if (!string.IsNullOrWhiteSpace(TmpImagePath))
        {
            request.TempImagePath = TmpImagePath;
        }

        var response = await client.Users.Update(request).Handle(l => IsLoading = l);

        if (response.IsSuccess && response.Data)
        {
            AuthStore.Instance.SetAuth(
                AuthStore.Instance.Token,
                FullName,
                Username,
                UserId,
                (long)AuthStore.Instance.Permissions); // mavjud ruxsatlarni saqlab qolamiz

            return true;
        }
        else
        {
            ErrorMessage = response.Message ?? "Saqlashda xatolik!";
            return false;
        }
    }
}

namespace Forex.Wpf.ViewModels;

using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.SemiProducts;
using Forex.ClientService.Services;
using Forex.Wpf.Enums;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class SemiProductViewModel(ForexClient client) : ViewModelBase
{
    private readonly IApiSemiProduct api = client.SemiProduct;

    public ObservableCollection<ItemFormData> Items { get; } = default!;
    public ItemFormData CurrentItem { get; set; } = default!;

    public void AddItemManually()
    {
        if (string.IsNullOrWhiteSpace(CurrentItem.Name) || CurrentItem.Quantity <= 0)
        {
            NotificationService.Show("Mahsulot nomi va miqdori to‘ldirilishi shart", NotificationType.Warning);
            return;
        }

        Items.Insert(0, CurrentItem);
        CurrentItem = new();
        OnPropertyChanged(nameof(CurrentItem));
        NotificationService.Show("Mahsulot qo‘shildi", NotificationType.Success);
    }

    public async Task SubmitIntakeAsync()
    {
        if (Items.Count == 0)
        {
            NotificationService.Show("Kamida bitta mahsulot qo‘shilishi kerak", NotificationType.Warning);
            return;
        }

        IsLoading = true;

        var command = new SemiProductIntakeCommand
        {
            SenderId = AuthStore.Instance.UserId,
            ManufactoryId = 1,
            EntryDate = DateTime.Now,
            TransferFeePerContainer = 0,
            Containers = [],
            Items = [.. Items]
        };

        var formData = MultipartBuilder.BuildIntake(command);
        var response = await api.CreateIntake(formData).Handle();

        IsLoading = false;

        if (response.StatusCode == 200)
        {
            Items.Clear();
            NotificationService.Show("Intake muvaffaqiyatli yuborildi", NotificationType.Success);
        }
        else
        {
            NotificationService.Show(response.Message ?? "Xatolik yuz berdi", NotificationType.Error);
        }
    }
}
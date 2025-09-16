namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using AutoMapper;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.SemiProducts;
using Forex.ClientService.Services;
using Forex.Wpf.Common.Commands;
using Forex.Wpf.Pages.Common;
using System.Windows.Input;

public class SemiProductPageViewModel : ViewModelBase
{
    private readonly ForexClient client = App.Client;
    private readonly IMapper mapper = App.Mapper;
    private SemiProductItemViewModel? selectedItem;
    private bool isEditing;

    public SemiProductPageViewModel()
    {
        Intake = new SemiProductIntakeViewModel();
        CurrentItem = new SemiProductItemViewModel();

        SaveCommand = new RelayCommand(SaveItem);
        DeleteCommand = new RelayCommand<SemiProductItemViewModel?>(RemoveItem);
        EditCommand = new RelayCommand<SemiProductItemViewModel?>(EditItem);
        SubmitCommand = new RelayCommand(async () => await SubmitAsync());
    }

    public SemiProductIntakeViewModel Intake { get; }

    private SemiProductItemViewModel? currentItem;
    public SemiProductItemViewModel CurrentItem
    {
        get => currentItem!;
        set => SetProperty(ref currentItem, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SubmitCommand { get; }

    private void SaveItem()
    {
        if (string.IsNullOrWhiteSpace(CurrentItem.Name) || CurrentItem.Quantity <= 0)
            return;

        if (IsEditing && selectedItem is not null)
        {
            selectedItem.Name = CurrentItem.Name;
            selectedItem.Code = CurrentItem.Code;
            selectedItem.Measure = CurrentItem.Measure;
            selectedItem.Quantity = CurrentItem.Quantity;
            selectedItem.CostPrice = CurrentItem.CostPrice;
            selectedItem.CostDelivery = CurrentItem.CostDelivery;
            selectedItem.TransferFee = CurrentItem.TransferFee;
            selectedItem.PhotoPath = CurrentItem.PhotoPath;
        }
        else
        {
            Intake.Items.Add(new SemiProductItemViewModel
            {
                Name = CurrentItem.Name,
                Code = CurrentItem.Code,
                Measure = CurrentItem.Measure,
                Quantity = CurrentItem.Quantity,
                CostPrice = CurrentItem.CostPrice,
                CostDelivery = CurrentItem.CostDelivery,
                TransferFee = CurrentItem.TransferFee,
                PhotoPath = CurrentItem.PhotoPath
            });
        }

        CurrentItem = new SemiProductItemViewModel();
        IsEditing = false;
        selectedItem = null;
    }

    private void RemoveItem(SemiProductItemViewModel? item)
    {
        if (item is null) return;
        Intake.Items.Remove(item);
    }

    private void EditItem(SemiProductItemViewModel? item)
    {
        if (item is null) return;

        selectedItem = item;
        IsEditing = true;

        CurrentItem = new SemiProductItemViewModel
        {
            Name = item.Name,
            Code = item.Code,
            Measure = item.Measure,
            Quantity = item.Quantity,
            CostPrice = item.CostPrice,
            CostDelivery = item.CostDelivery,
            TransferFee = item.TransferFee,
            PhotoPath = item.PhotoPath
        };
    }


    private async Task SubmitAsync()
    {
        var command = mapper.Map<SemiProductIntakeCommand>(Intake);
        using var form = MultipartBuilder.BuildIntake(command);

        var response = await client.SemiProduct.CreateIntake(form).Handle();

        if (response.StatusCode == 200)
        {
            Intake.Items.Clear();
            Intake.Containers.Clear();
            // TODO: NotificationService.Success("Muvaffaqiyatli yuborildi");
        }
        else
        {
            // TODO: NotificationService.Error(response.Message);
        }
    }

    public bool IsEditing
    {
        get => isEditing;
        set => SetProperty(ref isEditing, value);
    }
}
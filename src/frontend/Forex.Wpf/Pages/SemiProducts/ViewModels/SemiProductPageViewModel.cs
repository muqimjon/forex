namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using AutoMapper;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Invoices;
using Forex.ClientService.Models.Manufactories;
using Forex.ClientService.Models.SemiProducts;
using Forex.ClientService.Models.Users;
using Forex.ClientService.Services;
using Forex.Wpf.Common.Commands;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class SemiProductPageViewModel : ViewModelBase
{
    private readonly ForexClient client = App.Client;
    private readonly IMapper mapper = App.Mapper;
    private SemiProductItemViewModel? selectedItem;

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
    public ObservableCollection<ManufactoryResponse> Manufactories { get; } = [];

    public async Task LoadManufactoriesAsync()
    {
        var response = await client.Manufactory
            .GetAll()
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data is not null)
        {
            Manufactories.Clear();
            foreach (var m in response.Data)
                Manufactories.Add(m);
        }
        else
            ErrorMessage = response.Message ?? "Manufakturalar yuklanmadi";
    }

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
        var command = mapper.Map<InvoiceRequest>(Intake);
        using var form = MultipartBuilder.BuildIntake(command);

        var response = await client.SemiProduct
            .CreateIntake(form)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.StatusCode == 200)
        {
            Intake.Items.Clear();
            Intake.Containers.Clear();
            SuccessMessage = "Muvaffaqiyatli yuborildi";
        }
        else
        {
            ErrorMessage = response.Message;
        }
    }

    #region Suppliers

    public ObservableCollection<UserResponse> Suppliers { get; } = [];
    private UserResponse? selectedSupplier;
    public UserResponse? SelectedSupplier
    {
        get => selectedSupplier;
        set
        {
            if (SetProperty(ref selectedSupplier, value) && value is not null)
            {
                Intake.SenderId = value.Id;
                selectedSupplier!.Id = value.Id;
                selectedSupplier.Phone = value.Phone;
                selectedSupplier.Email = value.Email;
                selectedSupplier.Address = value.Address;
            }
        }
    }

    public async Task LoadSuppliersAsync()
    {
        var request = new FilteringRequest
        {
            Filters = new Dictionary<string, List<string>>
            {
                ["Role"] = ["taminotchi"]
            }
        };

        var response = await client.Users
            .Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data is not null)
        {
            Suppliers.Clear();
            foreach (var user in response.Data)
                Suppliers.Add(user);
        }
        else
        {
            ErrorMessage = response.Message ?? "Ta’minotchilar yuklanmadi";
        }
    }

    #endregion

    #region Semi product

    public ObservableCollection<SemiProductResponse> SemiProducts { get; } = [];

    private SemiProductResponse? selectedSemiProduct;
    public SemiProductResponse? SelectedSemiProduct
    {
        get => selectedSemiProduct;
        set
        {
            if (SetProperty(ref selectedSemiProduct, value) && value is not null)
            {
                CurrentItem.Name = value.Name;
                CurrentItem.Code = value.Code;
                CurrentItem.Measure = value.Measure;
            }
        }
    }

    public ICommand LoadSemiProductsCommand => new RelayCommand(async () => await EnsureSemiProductsAsync());

    private async Task EnsureSemiProductsAsync()
    {
        var response = await client.SemiProduct
            .GetAll()
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data is not null)
        {
            SemiProducts.Clear();
            foreach (var m in response.Data)
                SemiProducts.Add(m);
        }
        else
            ErrorMessage = response.Message ?? "Manufakturalar yuklanmadi";
    }

    #endregion

    private bool isEditing;
    public bool IsEditing
    {
        get => isEditing;
        set => SetProperty(ref isEditing, value);
    }
}
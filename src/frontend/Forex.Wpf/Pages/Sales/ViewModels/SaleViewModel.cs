namespace Forex.Wpf.Pages.Sales.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Models.Users;
using Forex.Wpf.Pages.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

public partial class SaleViewModel(ForexClient _client) : ViewModelBase
{


    // 🗓 Sana
    [ObservableProperty] private DateTime operationDate = DateTime.Now;

    // 👤 Mijoz
    [ObservableProperty] private UserResponse? selectedCustomer;
    [ObservableProperty] private ObservableCollection<UserResponse> customers = new();

    // 💵 Hisoblar
    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private decimal? finalAmount;
    [ObservableProperty] private string description = string.Empty;

    // 🧾 Joriy mijoz ma’lumotlari
    [ObservableProperty] private decimal? beginBalance;
    [ObservableProperty] private decimal? lastBalance;
    [ObservableProperty] private string phone = string.Empty;

    // 📦 Hozir kiritilayotgan mahsulot
    [ObservableProperty] private SaleItemViewModel currentSaleItem = new();

    // 🧮 Ro‘yxat (DataGrid uchun)
    [ObservableProperty] private ObservableCollection<SaleItemViewModel> saleItems = new();


    // 🔄 Backend’dan foydalanuvchilarni olish
    public async Task LoadUsersAsync()
    {
        try
        {
            var response = await _client.Users.GetAll();
            if (response.IsSuccess && response.Data != null)
            {
                Customers = new ObservableCollection<UserResponse>(response.Data);
            }
            else
            {
                WarningMessage = "Foydalanuvchilarni yuklashda xatolik.";
            }
        }
        catch (Exception ex)
        {
            WarningMessage = $"Server bilan aloqa yo'q: {ex.Message}";
        }
    }

    public async Task LoadProductsAsync()
    {
        try
        {
            var response = await _client.Users.GetAll();
            if (response.IsSuccess && response.Data != null)
            {
                Customers = new ObservableCollection<UserResponse>(response.Data);
            }
            else
            {
                WarningMessage = "Foydalanuvchilarni yuklashda xatolik.";
            }
        }
        catch (Exception ex)
        {
            WarningMessage = $"Server bilan aloqa yo'q: {ex.Message}";
        }
    }

    // ➕ Mahsulot qo‘shish
    [RelayCommand]
    private void Add()
    {
        if (CurrentSaleItem == null || CurrentSaleItem.Count <= 0)
        {
            WarningMessage = "Mahsulot tanlanmagan yoki miqdor noto‘g‘ri!";
            return;
        }

        SaleItems.Add(CurrentSaleItem);
        CurrentSaleItem.PropertyChanged += Item_PropertyChanged;
        RecalculateTotals();
        CurrentSaleItem = new SaleItemViewModel();
    }

    // 📤 Sotuvni yuborish
    [RelayCommand]
    private void Submit()
    {
        if (SaleItems.Count == 0)
        {
            WarningMessage = "Hech qanday mahsulot kiritilmagan!";
            return;
        }

        SuccessMessage = $"Savdo muvaffaqiyatli yuborildi. Mahsulotlar soni: {SaleItems.Count}";
    }

    #region CalculateTotalAmount 

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SaleItemViewModel.TotalAmount))
        {
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        TotalAmount = SaleItems.Sum(x => x.TotalAmount);
        FinalAmount = TotalAmount; // agar chegirma yoki qo‘shimcha bo‘lsa shu yerda hisoblanadi
    }

    #endregion CalculateTotalAmount

}
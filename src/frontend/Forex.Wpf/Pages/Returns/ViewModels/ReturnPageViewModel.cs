namespace Forex.Wpf.Pages.Returns.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

public partial class ReturnPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ForexClient client = App.AppHost!.Services.GetRequiredService<ForexClient>();
    private readonly IMapper mapper = App.AppHost!.Services.GetRequiredService<IMapper>();

    // Sana oralig'idagi barcha qaytarishlar (mijoz bo'yicha filtr + sahifalash mahalliy bajariladi).
    private List<ReturnResponse> _all = [];
    private bool _suppressCustomerReload;

    public ReturnPageViewModel()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(BeginDate) or nameof(EndDate))
                _ = OnDateRangeChangedAsync();
        };

        _ = LoadReturnsAsync();
    }

    [ObservableProperty] private ObservableCollection<ReturnResponse> returns = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> filteredCustomers = [];
    [ObservableProperty] private ReturnResponse? selectedReturn;

    [ObservableProperty] private DateTime beginDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime endDate = DateTime.Today;
    [ObservableProperty] private UserViewModel? selectedCustomer;
    [ObservableProperty] private string customerSearchText = string.Empty;

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int totalPages = 1;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private int pageSize = 20;
    [ObservableProperty] private ObservableCollection<object> visiblePageNumbers = [];

    [ObservableProperty] private decimal totalReturnAmount;
    [ObservableProperty] private int totalItemsReturned;

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    partial void OnCurrentPageChanged(int value) { UpdateVisiblePageNumbers(); ApplyPage(); }
    partial void OnTotalPagesChanged(int value) => UpdateVisiblePageNumbers();
    partial void OnCustomerSearchTextChanged(string value) => ApplyCustomerFilter();

    partial void OnSelectedCustomerChanged(UserViewModel? value)
    {
        if (_suppressCustomerReload) return;
        CurrentPage = 1;
        ApplyFilterAndPage();
    }

    #region Load data

    private async Task OnDateRangeChangedAsync()
    {
        CurrentPage = 1;
        await LoadReturnsAsync();
    }

    private async Task LoadReturnsAsync()
    {
        FilteringRequest request = new()
        {
            Page = 0,
            PageSize = 0,
            SortBy = "date",
            Descending = true,
            Filters = new()
            {
                ["date"] = [$">={BeginDate:o}", $"<{EndDate.AddDays(1):o}"],
                ["customer"] = ["include"],
                ["returnItems"] = ["include:productType.product"]
            }
        };

        var response = await client.Returns.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data is not null)
        {
            _all = response.Data;

            var customers = _all
                .Where(r => r.Customer is not null)
                .Select(r => r.Customer!)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            AvailableCustomers = mapper.Map<ObservableCollection<UserViewModel>>(customers);

            _suppressCustomerReload = true;
            var keepId = SelectedCustomer?.Id;
            SelectedCustomer = keepId is null ? null : AvailableCustomers.FirstOrDefault(c => c.Id == keepId);
            _suppressCustomerReload = false;

            ApplyCustomerFilter();
            ApplyFilterAndPage();
        }
        else if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Qaytarishlarni yuklashda xatolik.";
        }
    }

    private List<ReturnResponse> FilteredAll()
        => SelectedCustomer is null
            ? _all
            : _all.Where(r => r.CustomerId == SelectedCustomer.Id).ToList();

    private void ApplyFilterAndPage()
    {
        var filtered = FilteredAll();

        TotalCount = filtered.Count;
        TotalReturnAmount = filtered.Sum(r => r.BaseAmount);
        TotalItemsReturned = filtered.Sum(r => r.TotalCount);
        TotalPages = PageSize > 0 ? Math.Max(1, (int)Math.Ceiling((double)filtered.Count / PageSize)) : 1;

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        ApplyPage();
        UpdateVisiblePageNumbers();
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private void ApplyPage()
    {
        var slice = FilteredAll().Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        Returns = new ObservableCollection<ReturnResponse>(slice);
    }

    private void ApplyCustomerFilter()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            FilteredCustomers = new ObservableCollection<UserViewModel>(AvailableCustomers);
            return;
        }

        var filtered = AvailableCustomers.Where(c =>
            TransliterationHelper.ContainsIgnoreScript(c.Name, CustomerSearchText) ||
            TransliterationHelper.ContainsIgnoreScript(c.Phone, CustomerSearchText));

        FilteredCustomers = new ObservableCollection<UserViewModel>(filtered);
    }

    private void UpdateVisiblePageNumbers()
    {
        var pages = new List<object>();

        if (TotalPages <= 7)
        {
            for (int i = 1; i <= TotalPages; i++) pages.Add(i);
        }
        else
        {
            pages.Add(1);
            if (CurrentPage > 4) pages.Add("...");

            int start = Math.Max(2, CurrentPage - 1);
            int end = Math.Min(TotalPages - 1, CurrentPage + 1);
            if (CurrentPage < 5) end = 5;
            else if (CurrentPage > TotalPages - 4) start = TotalPages - 4;

            for (int i = start; i <= end; i++) pages.Add(i);

            if (CurrentPage < TotalPages - 3) pages.Add("...");
            pages.Add(TotalPages);
        }

        VisiblePageNumbers = new ObservableCollection<object>(pages);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task PreviewReturn(ReturnResponse? ret)
    {
        if (ret is null) return;

        IsLoading = true;
        try
        {
            var printVm = App.AppHost!.Services.GetRequiredService<AddReturnPageViewModel>();
            await printVm.LoadReturnForEditAsync(ret.Id);
            await printVm.ShowPrintPreview();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedCustomer = null;
        CustomerSearchText = string.Empty;
        BeginDate = DateTime.Today.AddDays(-7);
        EndDate = DateTime.Today;
    }

    [RelayCommand] private void GoToFirstPage() { if (CurrentPage != 1) CurrentPage = 1; }
    [RelayCommand] private void GoToPreviousPage() { if (CanGoPrevious) CurrentPage--; }
    [RelayCommand] private void GoToNextPage() { if (CanGoNext) CurrentPage++; }
    [RelayCommand] private void GoToLastPage() { if (CurrentPage != TotalPages) CurrentPage = TotalPages; }

    [RelayCommand]
    private void GoToPage(object? parameter)
    {
        if (parameter is int page && page >= 1 && page <= TotalPages && page != CurrentPage)
            CurrentPage = page;
    }

    [RelayCommand]
    private async Task Delete(ReturnResponse? value)
    {
        if (value is null) return;

        var result = MessageBox.Show(
            $"Qaytarishni o'chirishni tasdiqlaysizmi?\n\nMijoz: {value.Customer?.Name}\nSana: {value.Date:dd.MM.yyyy}\nSumma: {value.TotalAmount:N2}",
            "O'chirishni tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No) return;

        var response = await client.Returns.Delete(value.Id).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            _all.Remove(value);
            SuccessMessage = "Qaytarish muvaffaqiyatli o'chirildi";
            ApplyFilterAndPage();
        }
        else
        {
            ErrorMessage = response.Message ?? "Qaytarishni o'chirishda xatolik";
        }
    }

    #endregion

    #region Navigation

    public void OnNavigatedTo() => _ = LoadReturnsAsync();

    public void OnNavigatedFrom() { }

    #endregion
}

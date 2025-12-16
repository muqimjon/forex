namespace Forex.Wpf.Windows.OverdueAccountsWindow.ViewModel;

using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

public partial class OverdueAccountsViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;


    public OverdueAccountsViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;
        _ = LoadAccountsAsync();
    }

    [ObservableProperty] private ObservableCollection<UserAccountViewModel> allAccountsSource = [];
    [ObservableProperty] private ObservableCollection<UserAccountViewModel> aviableUserAccounts = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> aviableUsers = [];
    [ObservableProperty] private UserViewModel? selectedCustomer;


    partial void OnSelectedCustomerChanged(UserViewModel? oldValue, UserViewModel? newValue)
    {
        ApplyFilter();
    }

    private async Task LoadAccountsAsync()
    {
        var allAccountsResult = await client.UserAccounts.GetAllAsync()
            .Handle(isLoading => IsLoading = isLoading);

        var allUsersResult = await client.Users.GetAllAsync();

        if (!allAccountsResult.IsSuccess || !allUsersResult.IsSuccess)
        {
            WarningMessage = "Ma'lumotlarni yuklashda xatolik.";
            return;
        }

        var excludedUserRoles = new ClientService.Enums.UserRole[]
        {
        ClientService.Enums.UserRole.Hodim,
        };

        var allowedUserIds = allUsersResult.Data
            .Where(u => !excludedUserRoles.Contains(u.Role))
            .Select(u => u.Id)
            .ToHashSet();


        var finalFilteredAccountsDto = allAccountsResult.Data
            .Where(a =>
                allowedUserIds.Contains(a.UserId) &&
                a.DueDate.HasValue &&
                a.DueDate.Value.Date <= DateTime.Today)
            .ToList();

        var accountsVm = mapper.Map<List<UserAccountViewModel>>(finalFilteredAccountsDto);

        var uniqueUserIdsInAccounts = accountsVm.Select(a => a.UserId).Distinct().ToList();

        var availableUsersList = allUsersResult.Data
            .Where(u => uniqueUserIdsInAccounts.Contains(u.Id))
            .ToList();

        var aviableUsersVm = mapper.Map<List<UserViewModel>>(availableUsersList);

        foreach (var acc in accountsVm)
        {
            var usr = aviableUsersVm.FirstOrDefault(u => u.Id == acc.UserId);
            acc.User = usr ?? new UserViewModel { Id = acc.UserId, Name = "Noma'lum foydalanuvchi" };
        }

        AviableUsers = new ObservableCollection<UserViewModel>(aviableUsersVm);
        AllAccountsSource = new ObservableCollection<UserAccountViewModel>(accountsVm);
        AviableUserAccounts = AllAccountsSource;
    }
    private void ApplyFilter()
    {
        IEnumerable<UserAccountViewModel> query = AllAccountsSource;

        if (SelectedCustomer != null)
        {
            query = query.Where(a => a.UserId == SelectedCustomer.Id);
        }

        AviableUserAccounts = new ObservableCollection<UserAccountViewModel>(query);
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        if (AviableUserAccounts == null || !AviableUserAccounts.Any())
        {
            MessageBox.Show("Excelga eksport qilish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Excel fayllari (*.xlsx)|*.xlsx",
            FileName = $"Qarzdor_mijozlar_{DateTime.Now:dd.MM.yyyy}.xlsx"
        };

        if (saveDialog.ShowDialog() != true) return;

        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Qarzdorlar");

            // Sarlavha
            worksheet.Cell(1, 1).Value = "Qarzni to'lash muddati o'tib ketgan mijozlar ro'yxati";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 5).Merge();

            // Sana
            worksheet.Cell(2, 1).Value = $"Sana: {DateTime.Now:dd.MM.yyyy}";
            worksheet.Range(2, 1, 2, 5).Merge();

            // Headerlar
            string[] headers = { "Mijoz", "Telefon", "Qarzdorlik", "Muddat", "Izoh" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(235, 235, 235);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Ma'lumotlar
            int row = 5;
            foreach (var item in AviableUserAccounts)
            {
                worksheet.Cell(row, 1).Value = item.User?.Name ?? "";
                worksheet.Cell(row, 2).Value = item.User?.Phone ?? "";
                worksheet.Cell(row, 3).Value = item.Balance;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 4).Value = item.DueDate?.ToString("dd.MM.yyyy") ?? "";
                worksheet.Cell(row, 5).Value = item.Description ?? "";

                worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;
            }

            // Ustun eni
            worksheet.Column(1).Width = 25;
            worksheet.Column(2).Width = 18;
            worksheet.Column(3).Width = 16;
            worksheet.Column(4).Width = 14;
            worksheet.Column(5).Width = 40;

            // Hizalash
            worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Column(5).Style.Alignment.WrapText = true;

            workbook.SaveAs(saveDialog.FileName);
            MessageBox.Show("Excel fayl muvaffaqiyatli saqlandi!", "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excelga saqlashda xatolik: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Print()
    {
        if (AviableUserAccounts == null || !AviableUserAccounts.Any())
        {
            MessageBox.Show("Chop etish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fixedDoc = CreateFixedDocumentForOverdue();

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintDocument(fixedDoc.DocumentPaginator, "Qarzdor mijozlar ro'yxati");
        }
    }

    [RelayCommand]
    private async Task SaveAsync(UserAccountViewModel account)
    {
        if (account == null) return;

        try
        {
            var dto = new UserAccountRequest
            {
                Id = account.Id,
                Description = account.Description,

                DueDate = account.DueDate.HasValue
                    ? DateTime.SpecifyKind(account.DueDate.Value, DateTimeKind.Utc)
                    : null,

                Balance = account.Balance,
                OpeningBalance = account.OpeningBalance,
                Discount = account.Discount,
                UserId = account.UserId,
                CurrencyId = account.CurrencyId,
            };

            var result = await client.UserAccounts.UpdateAsync(dto)
                .Handle(isLoading => IsLoading = isLoading);

            if (!result.IsSuccess)
            {
                MessageBox.Show("Saqlashda xatolik yuz berdi.", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCustomer = null;
    }

    [RelayCommand]
    private void Preview()
    {
        ShowPreview();
    }

    private void ShowPreview()
    {
        if (AviableUserAccounts == null || !AviableUserAccounts.Any())
        {
            MessageBox.Show("Ko‘rsatish uchun ma’lumot yo‘q.", "Eslatma",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fixedDoc = CreateFixedDocumentForOverdue();

        // Xato berayotgan joy: 'Window' o'rniga System.Windows.Window dan foydalaning
        var previewWindow = new System.Windows.Window
        {
            Title = "Qarzdor mijozlar - Oldindan ko‘rish",
            Width = 900,
            Height = 780,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen // Bu joyni ham to'g'irlash kerak bo'lishi mumkin
        };

        var viewer = new DocumentViewer
        {
            Document = fixedDoc,
            Margin = new System.Windows.Thickness(10) // Thickness uchun ham xato bo'lishi mumkin
        };

        previewWindow.Content = viewer;
        previewWindow.ShowDialog();
    }
    private FixedDocument CreateFixedDocumentForOverdue()
    {
        double pageWidth = 793.7;   // A4
        double pageHeight = 1122.5;

        double leftMargin = 40;     // Standart chap margin
        double rightMargin = 40;    // O‘ng ham shu
        double topMargin = 20;

        var fixedDoc = new FixedDocument();
        fixedDoc.DocumentPaginator.PageSize = new Size(pageWidth, pageHeight);

        var page = new FixedPage
        {
            Width = pageWidth,
            Height = pageHeight,
            Background = Brushes.White
        };

        // Title
        var title = new TextBlock
        {
            Text = "Qarzni to'lash muddati o'tib ketgan mijozlar ro'yxati",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Width = pageWidth - leftMargin - rightMargin
        };
        FixedPage.SetTop(title, topMargin);
        FixedPage.SetLeft(title, leftMargin);
        page.Children.Add(title);

        // Date
        var date = new TextBlock
        {
            Text = $"Sana: {DateTime.Now:dd.MM.yyyy}",
            FontSize = 14,
            TextAlignment = TextAlignment.Left,
            Width = pageWidth - leftMargin - rightMargin
        };
        FixedPage.SetTop(date, topMargin + 35);
        FixedPage.SetLeft(date, leftMargin);
        page.Children.Add(date);

        // GRID – A4 enini to‘liq egallasin
        var grid = new Grid
        {
            Width = pageWidth - leftMargin - rightMargin,
            Margin = new Thickness(leftMargin, topMargin + 70, rightMargin, 30)
        };

        // Ustun nisbatlari
        // Ustun nisbatlari — Telefon kengaytirildi
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.0, GridUnitType.Star) }); // Mijoz
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Telefon — KENGAYTIRILDI
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Qarzdorlik
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) }); // Muddat
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4.0, GridUnitType.Star) }); // Izoh — balanslangan
        int row = 0;
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        string[] headers = { "Mijoz", "Telefon", "Qarzdorlik", "Muddat", "Izoh" };
        TextAlignment[] headerAlignments =
        {
        TextAlignment.Left,
        TextAlignment.Center,
        TextAlignment.Right,
        TextAlignment.Center,
        TextAlignment.Left
    };

        // Header
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = CreateOverdueCell(headers[i], true, headerAlignments[i]);
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, i);
            grid.Children.Add(cell);
        }

        // Rows
        foreach (var item in AviableUserAccounts)
        {
            row++;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] values =
            {
            item.User?.Name ?? "",
            item.User?.Phone ?? "",
            item.Balance.ToString("N2"),
            item.DueDate?.ToString("dd.MM.yyyy") ?? "",
            item.Description ?? ""
        };

            TextAlignment[] aligns =
            {
            TextAlignment.Left,
            TextAlignment.Center,
            TextAlignment.Right,
            TextAlignment.Center,
            TextAlignment.Left
        };

            for (int i = 0; i < values.Length; i++)
            {
                var cell = CreateOverdueCell(values[i], false, aligns[i]);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, i);
                grid.Children.Add(cell);
            }
        }

        page.Children.Add(grid);

        var pageContent = new PageContent();
        ((IAddChild)pageContent).AddChild(page);
        fixedDoc.Pages.Add(pageContent);

        return fixedDoc;
    }
    private Border CreateOverdueCell(string text, bool isHeader, TextAlignment alignment = TextAlignment.Left)
    {
        var border = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Background = isHeader ? new SolidColorBrush(Color.FromRgb(235, 235, 235)) : Brushes.White,
            // Padding ni 3 px ga tushirdik — eni kengayadi
            Padding = new Thickness(3, 4, 3, 4)
        };

        var tb = new TextBlock
        {
            Text = text,
            FontSize = isHeader ? 13 : 12,
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Center,
            // Uzun matn uchun avto-o'rash
            TextWrapping = TextWrapping.Wrap
        };

        border.Child = tb;
        return border;
    }
}
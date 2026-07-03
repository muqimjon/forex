namespace Forex.Wpf.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Common.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

// Barkod bo'limidagi modal ko'rinishini qayta ishlatuvchi yengil ViewModel.
// Bitta razmer (ProductType) uchun QOP/TO'PLAM barkodini oldindan ko'rish + chop etish.
public partial class BarcodeLabelPreviewViewModel : ObservableObject
{
    private readonly int bundleItemCount;
    private readonly int packItemCount;
    private readonly string? qopBarcode;
    private readonly string? packBarcode;

    public string Title { get; }
    public string Size { get; }
    public string? ImagePath { get; }

    [ObservableProperty] private ObservableCollection<string> units = [];
    [ObservableProperty] private string selectedUnit = "Qop";
    [ObservableProperty] private string unitLabel = "QOP";
    [ObservableProperty] private int pairs;
    [ObservableProperty] private string? barcode;
    [ObservableProperty] private ImageSource? barcodeImage;
    [ObservableProperty] private int copies = 1;

    public bool HasMultipleUnits => Units.Count > 1;

    // Chop etish muvaffaqiyatli bo'lganda oynani yopish uchun.
    public event Action? CloseRequested;

    public BarcodeLabelPreviewViewModel(
        string title, string size,
        int bundleItemCount, int packItemCount,
        string? qopBarcode, string? packBarcode, string? imagePath)
    {
        Title = title;
        Size = size;
        this.bundleItemCount = bundleItemCount;
        this.packItemCount = packItemCount;
        this.qopBarcode = qopBarcode;
        this.packBarcode = packBarcode;
        ImagePath = imagePath;

        if (!string.IsNullOrWhiteSpace(qopBarcode)) Units.Add("Qop");
        if (!string.IsNullOrWhiteSpace(packBarcode) && packItemCount > 0) Units.Add("To'plam");
        if (Units.Count == 0) Units.Add("Qop");

        SelectedUnit = Units[0];
        Apply(SelectedUnit);
    }

    partial void OnSelectedUnitChanged(string value) => Apply(value);

    private void Apply(string unit)
    {
        if (unit == "To'plam")
        {
            UnitLabel = "TO'PLAM";
            Pairs = packItemCount;
            Barcode = packBarcode;
        }
        else
        {
            UnitLabel = "QOP";
            Pairs = bundleItemCount;
            Barcode = qopBarcode;
        }

        BarcodeImage = string.IsNullOrWhiteSpace(Barcode) ? null : BarcodeImageService.Render(Barcode);
    }

    [RelayCommand]
    private void IncrementCopies()
    {
        if (Copies < 500) Copies++;
    }

    [RelayCommand]
    private void DecrementCopies()
    {
        if (Copies > 1) Copies--;
    }

    [RelayCommand]
    private void Print()
    {
        if (string.IsNullOrWhiteSpace(Barcode)) return;

        var count = Math.Clamp(Copies, 1, 500);
        Copies = count;

        var label = new LabelItem(Title, Size, UnitLabel, Pairs, Barcode);
        if (LabelPrintService.Print([label], AppPreferences.Instance.LabelPrinter, count))
            CloseRequested?.Invoke();
    }
}

namespace Forex.Wpf.Pages.Reports.ViewModels;

public partial class DebtorCreditorItemViewModel
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Address { get; set; }

    public decimal DebtorAmount { get; set; }
    public decimal CreditorAmount { get; set; }
}

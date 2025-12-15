namespace Forex.Wpf.ViewModels;

using Forex.Wpf.Pages.Common;

public class TurnoversViewModel : ViewModelBase
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
}

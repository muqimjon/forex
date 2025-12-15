namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Globalization;

public partial class UserAccountViewModel : ViewModelBase
{
    // ... boshqa xususiyatlar (o'zgarishsiz)
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }
    public string? Description { get; set; } = string.Empty;


    public long UserId { get; set; }
    public UserViewModel User { get; set; } = default!;
    public long CurrencyId { get; set; }
    public CurrencyViewModel Currency { get; set; } = default!;

    // >>> Yangi Xususiyatni Qo'shish Boshlanishi <<<

    private DateTime? _dueDate; // Backing field
    public DateTime? DueDate
    {
        get => _dueDate;
        set
        {
            if (_dueDate != value)
            {
                _dueDate = value;

                // >>> DIQQAT: DueDateString maydonini to'g'ridan-to'g'ri yangilash <<<
                if (_dueDate.HasValue)
                {
                    // DueDateString fieldini to'g'ridan-to'g'ri yangilash (loopni oldini oladi)
                    _dueDateString = _dueDate.Value.ToString("dd.MM.yyyy");
                }
                else
                {
                    _dueDateString = string.Empty;
                }

                OnPropertyChanged(nameof(DueDate));
                OnPropertyChanged(nameof(DueDateString)); // UI ni yangilash
            }
        }
    }

    // ... (CustomerId, Customer, CurrencyId, Currency o'zgarishsiz)

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DueDate))]
    private string _dueDateString = string.Empty;

    // ...
    partial void OnDueDateStringChanged(string value)
    {
        // 1. Matndan faqat raqamlarni ajratib olish (Code-Behind'dagi vazifa)
        string numericText = new string(value.Where(char.IsDigit).ToArray());

        // 2. Formatlash (Code-Behind'dagi vazifa)
        if (numericText.Length > 8)
            numericText = numericText.Substring(0, 8);

        if (numericText.Length >= 2) numericText = numericText.Insert(2, ".");
        if (numericText.Length >= 5) numericText = numericText.Insert(5, ".");

        // Agar kiritilgan matn allaqachon to'g'ri formatlangan bo'lsa, qayta o'rnatish shart emas
        if (value != numericText)
        {
            // Rekursiv chaqiruvni oldini olish uchun maydonni to'g'ridan-to'g'ri o'rnatish
            _dueDateString = numericText;
            OnPropertyChanged(nameof(DueDateString));

            // Agar qiymat o'zgargan bo'lsa, bu yerda chiqib ketamiz, chunki
            // to'g'ri formatlangandan keyin OnDueDateStringChanged yana chaqiriladi
            // va pastdagi tekshiruvlar toza qiymat uchun ishlaydi.
            if (string.IsNullOrEmpty(numericText) || numericText.Length < 10)
            {
                // Agar to'liq sana formatlanmagan bo'lsa, DueDate ni null qilamiz.
                _dueDate = null;
                OnPropertyChanged(nameof(DueDate));
                return;
            }
        }

        // 3. Tekshirish mantiqi (avvalgi mantiq to'g'ri)
        if (DateTime.TryParseExact(numericText, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime parsedDate))
        {
            if (parsedDate.Date >= DateTime.Today.Date)
            {
                _dueDate = parsedDate;
                OnPropertyChanged(nameof(DueDate));
            }
            else
            {
                // Bugundan kichik bo'lsa, tozalaymiz va xato bildiramiz.
                _dueDate = null;
                OnPropertyChanged(nameof(DueDate));
                _dueDateString = string.Empty;
                OnPropertyChanged(nameof(DueDateString));
            }
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
            _dueDate = null;
            OnPropertyChanged(nameof(DueDate));
        }
        else if (numericText.Length == 10)
        {
            // Noto'g'ri sana, lekin to'liq yozilgan (masalan, 32.01.2025)
            _dueDate = null;
            OnPropertyChanged(nameof(DueDate));
            _dueDateString = string.Empty;
            OnPropertyChanged(nameof(DueDateString));
        }
    }
}
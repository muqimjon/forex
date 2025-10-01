using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Forex.Wpf.Resources.UserControls;

/// <summary>
/// Interaction logic for UserCalendar.xaml
/// </summary>
public partial class UserCalendar : UserControl
{
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(UserCalendar), new PropertyMetadata(DateTime.Now, OnSelectedDateChanged));

    private static readonly Regex _dateInputRegex = new Regex("[0-8]", RegexOptions.Compiled);
    private static readonly Regex _dateFormatRegex = new Regex(@"^(?:\d{2}\.\d{2}\.\d{2,4})?$", RegexOptions.Compiled);

    public UserCalendar()
    {
        InitializeComponent();
        dateTextBox.PreviewTextInput += DateTextBox_PreviewTextInput;
        dateTextBox.TextChanged += DateTextBox_TextChanged;
        dateTextBox.PreviewLostKeyboardFocus += DateTextBox_PreviewLostKeyboardFocus;
        SetDefaultDate();
    }
    public static readonly DependencyProperty HintProperty =
    DependencyProperty.Register("Hint", typeof(string), typeof(UserCalendar), new PropertyMetadata(""));

    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserCalendar userCalendar && e.NewValue is DateTime newDate)
        {
            userCalendar.dateTextBox.Text = newDate.ToString("dd.MM.yyyy");
        }
        UserCalendar userCal = d as UserCalendar;
        userCal.dateTextBox.Focus();
        userCal.dateTextBox.SelectAll();
    }

    private void SetDefaultDate()
    {
        if (SelectedDate == null)
        {
            SelectedDate = DateTime.Now.Date;
        }
    }

    private void DateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = (TextBox)sender;

        // Текст, который будет после вставки нового символа
        if (!string.IsNullOrEmpty(textBox.SelectedText))
        {
            // Если есть выделенный текст — заменяем его новым вводом
            _ = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                  .Insert(textBox.SelectionStart, e.Text);
        }
        else
        {
            // Обычное добавление символа
            _ = textBox.Text.Insert(textBox.CaretIndex, e.Text);
        }

        if (dateTextBox.Text.Length > 10)
        {
            e.Handled = true; // Блокируем ввод, если длина текста уже соответствует полному формату даты
        }
        else
        {
            e.Handled = !IsValidDateInput(e.Text);
        }
    }

    private void DateTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Text.Length == 2)
            {
                textBox.Text += ".";
                textBox.CaretIndex = textBox.Text.Length;
            }
            if (textBox.Text.Length == 5)
            {
                textBox.Text += ".";
                textBox.CaretIndex = textBox.Text.Length;
            }

        }
    }

    private void DateTextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                SelectedDate = null;
                return;
            }
            if (!IsValidDateFormat(textBox.Text))
            {
                MessageBox.Show("Неверный формат даты. Используйте формат dd.MM.yyyy.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true; // Предотвращаем потерю фокуса
            }
            else
            {
                var parts = textBox.Text.Split('.');
                if (parts.Length == 3 && parts[2].Length == 2)
                {
                    parts[2] = "20" + parts[2];
                    textBox.Text = string.Join(".", parts);
                }

                if (!DateTime.TryParseExact(textBox.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out _))
                {
                    MessageBox.Show("Неверная дата. Пожалуйста, введите корректную дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true; // Предотвращаем потерю фокуса
                }
            }
        }
    }

    private void OpenCalendar_Click(object sender, RoutedEventArgs e)
    {
        calendarPopup.IsOpen = true;
    }

    private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (calendar.SelectedDate.HasValue)
        {
            SelectedDate = calendar.SelectedDate.Value;
            calendarPopup.IsOpen = false;
        }
    }

    private static bool IsValidDateInput(string input) => _dateInputRegex.IsMatch(input);

    private static bool IsValidDateFormat(string input) => _dateFormatRegex.IsMatch(input);
}

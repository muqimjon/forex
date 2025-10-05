namespace Forex.Wpf.Common.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class ComboBoxTabBehavior
{
    public static readonly DependencyProperty EnableTabOnSelectProperty =
        DependencyProperty.RegisterAttached(
            "EnableTabOnSelect",
            typeof(bool),
            typeof(ComboBoxTabBehavior),
            new UIPropertyMetadata(false, OnEnableTabOnSelectChanged));

    public static readonly DependencyProperty NextKeyProperty =
        DependencyProperty.RegisterAttached(
            "NextKey",
            typeof(Key),
            typeof(ComboBoxTabBehavior),
            new UIPropertyMetadata(Key.Tab));

    public static bool GetEnableTabOnSelect(DependencyObject obj) =>
        (bool)obj.GetValue(EnableTabOnSelectProperty);

    public static void SetEnableTabOnSelect(DependencyObject obj, bool value) =>
        obj.SetValue(EnableTabOnSelectProperty, value);

    public static Key GetNextKey(DependencyObject obj) =>
        (Key)obj.GetValue(NextKeyProperty);

    public static void SetNextKey(DependencyObject obj, Key value) =>
        obj.SetValue(NextKeyProperty, value);

    private static void OnEnableTabOnSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
            return;

        if ((bool)e.NewValue)
        {
            comboBox.DropDownClosed += ComboBox_DropDownClosed;
            comboBox.SelectionChanged += ComboBox_SelectionChanged;
            comboBox.PreviewKeyDown += ComboBox_PreviewKeyDown;
            comboBox.Unloaded += ComboBox_Unloaded;
            comboBox.GotFocus += ComboBox_GotFocus;

            SetComboBoxState(comboBox, new State());
        }
        else
        {
            comboBox.DropDownClosed -= ComboBox_DropDownClosed;
            comboBox.SelectionChanged -= ComboBox_SelectionChanged;
            comboBox.PreviewKeyDown -= ComboBox_PreviewKeyDown;
            comboBox.Unloaded -= ComboBox_Unloaded;
            comboBox.GotFocus -= ComboBox_GotFocus;

            ClearComboBoxState(comboBox);
        }
    }



    #region State per ComboBox
    private class State
    {
        public bool EnterPressed { get; set; }
        public bool MouseSelected { get; set; }
    }

    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "State",
            typeof(State),
            typeof(ComboBoxTabBehavior),
            new PropertyMetadata(null));

    private static void SetComboBoxState(ComboBox comboBox, State state) =>
        comboBox.SetValue(StateProperty, state);

    private static State? GetComboBoxState(ComboBox comboBox) =>
        comboBox.GetValue(StateProperty) as State;

    private static void ClearComboBoxState(ComboBox comboBox) =>
        comboBox.ClearValue(StateProperty);
    #endregion

    private static void ComboBox_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {

            if (sender is ComboBox comboBox)
            {
                if (!comboBox.IsDropDownOpen)
                {
                    comboBox.IsDropDownOpen = true;
                }
            }
        }
        catch { }
    }

    private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            var state = GetComboBoxState(comboBox);
            if (state != null)
                state.MouseSelected = true; // только при выборе элемента
        }
    }

    private static void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not ComboBox comboBox)
            return;

        var state = GetComboBoxState(comboBox);
        if (state == null) return;

        bool allSelected = false;
        if (TryGetEditableTextBox(comboBox, out var textBox))
        {
            allSelected = textBox.SelectionStart == 0 &&
                          textBox.SelectionLength == textBox.Text.Length &&
                          textBox.Text.Length > 0;
        }

        //// automatic DropDownOpen
        //if (!comboBox.IsDropDownOpen && GetKeyType(e))
        //{
        //    comboBox.IsDropDownOpen = true;
        //}

        // ENTER → Tab (вперёд)
        if (e.Key == Key.Enter)
        {
            state.EnterPressed = true;

            if (!comboBox.IsDropDownOpen)
            {
                e.Handled = true;
                DoNavigationForKey(comboBox, GetNextKey(comboBox));
            }
            else
            {
                //comboBox.IsDropDownOpen = false;
            }

            return;
        }

        // ← / ↑ → Shift+Tab (назад)
        if (e.Key == Key.Left || e.Key == Key.Up)
        {
            if (TryGetEditableTextBox(comboBox, out var tb))
            {
                if (tb.CaretIndex == 0 || string.IsNullOrEmpty(tb.Text) || allSelected)
                {
                    e.Handled = true;
                    if (comboBox.IsDropDownOpen)
                        comboBox.IsDropDownOpen = false;
                    MoveFocusPrevious(comboBox);
                }
            }
            else // не редактируемый ComboBox
            {
                e.Handled = true;
                MoveFocusPrevious(comboBox);
            }
        }

        // → / ↓ → Tab (вперёд)
        if (e.Key == Key.Right || e.Key == Key.Down)
        {
            if (TryGetEditableTextBox(comboBox, out var tb))
            {
                if (tb.CaretIndex == tb.Text.Length || allSelected)
                {
                    e.Handled = true;
                    if (comboBox.IsDropDownOpen)
                        comboBox.IsDropDownOpen = false;
                    DoNavigationForKey(comboBox, Key.Tab);
                }
            }
            else // не редактируемый ComboBox
            {
                e.Handled = true;
                DoNavigationForKey(comboBox, Key.Tab);
            }
        }
    }

    private static void ComboBox_DropDownClosed(object? sender, EventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            var state = GetComboBoxState(comboBox);
            if (state == null) return;

            if (state.EnterPressed)
            {
                state.EnterPressed = false;
                DoNavigationForKey(comboBox, GetNextKey(comboBox));
                return;
            }

            // теперь только если реально выбрали элемент
            if (state.MouseSelected)
            {
                state.MouseSelected = false;
                DoNavigationForKey(comboBox, GetNextKey(comboBox));
            }
        }
    }

    private static void ComboBox_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox comboBox)
            ClearComboBoxState(comboBox);
    }

    private static bool TryGetEditableTextBox(ComboBox comboBox, out TextBox textBox)
    {
        textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
        return textBox != null;
    }

    private static void DoNavigationForKey(ComboBox comboBox, Key key)
    {
        var focused = Keyboard.FocusedElement as UIElement ?? comboBox;
        if (focused == null) return;

        if (key == Key.Tab || key == Key.Enter || key == Key.Right || key == Key.Down)
            focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        else if (key == Key.Left || key == Key.Up)
            focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
    }

    private static void MoveFocusPrevious(ComboBox comboBox)
    {
        var focused = Keyboard.FocusedElement as UIElement ?? comboBox;
        if (focused != null)
            focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
    }

    //private static bool GetKeyType(KeyEventArgs e)
    //{
    //    // Буквы A–Z
    //    if (e.Key >= Key.A && e.Key <= Key.Z)
    //        return true;

    //    // Цифры верхнего ряда 0–9
    //    if (e.Key >= Key.D0 && e.Key <= Key.D9)
    //        return true;

    //    // Цифры NumPad 0–9
    //    if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
    //        return true;

    //    // Символы с клавиатуры (, . ; ' - = [ ] и т.п.)
    //    if (e.Key >= Key.Oem1 && e.Key <= Key.Oem102)
    //        return true;

    //    // Остальное можно игнорировать
    //    return false;
    //}
}
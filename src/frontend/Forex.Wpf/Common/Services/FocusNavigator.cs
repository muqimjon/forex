namespace Forex.Wpf.Common.Services;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

public static class FocusNavigator
{
    private static readonly Dictionary<UIElement, KeyEventHandler> RegisteredKeyDownHandlers = [];
    private static readonly Dictionary<Button, RoutedEventHandler> RegisteredClickHandlers = [];

    public static void RegisterElements(List<UIElement> focusOrder)
    {
        if (focusOrder is null || focusOrder.Count == 0)
            return;

        FocusElement(focusOrder[0]);

        foreach (var control in focusOrder)
        {
            if (RegisteredKeyDownHandlers.ContainsKey(control))
                UnregisterKeyDownInternal(control);

            if (control is FrameworkElement fe)
                fe.Unloaded += Element_Unloaded_Cleanup;

            void handler(object s, KeyEventArgs e)
            {
                bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                int currentIdx = focusOrder.IndexOf(control);
                UIElement? nextElement = null;

                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    if (control is Button btn && e.Key == Key.Enter && !shift)
                    {
                        btn.Command?.Execute(btn.CommandParameter);
                        btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        e.Handled = true;
                        return;
                    }

                    nextElement = shift ?
                        GetNextFocusableElement(focusOrder, currentIdx, isForward: false) :
                        GetNextFocusableElement(focusOrder, currentIdx, isForward: true);

                    if (nextElement is not null)
                    {
                        FocusElement(nextElement);
                        e.Handled = true;
                    }
                }

                if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
                {
                    if (control is TextBox tb)
                    {
                        HandleTextBoxPointerKeys(e, tb, currentIdx, shift, focusOrder, out nextElement);
                    }
                    else if (control is ComboBox cb)
                    {
                        HandleComboBoxPointerKeys(e, currentIdx, shift, focusOrder, out nextElement);
                    }
                    else
                    {
                        HandleGeneralPointerKeys(e, currentIdx, shift, focusOrder, out nextElement);
                    }

                    if (nextElement is not null)
                    {
                        FocusElement(nextElement);
                        e.Handled = true;
                    }
                }
            }

            control.PreviewKeyDown += handler;
            RegisteredKeyDownHandlers[control] = handler;
        }
    }

    public static void SetFocusRedirect(Button triggerElement, UIElement returnElement)
    {
        if (triggerElement is null || returnElement is null)
            return;

        if (RegisteredClickHandlers.ContainsKey(triggerElement))
            UnregisterClickInternal(triggerElement);

        if (triggerElement.Parent is FrameworkElement fe)
            fe.Unloaded += Element_Unloaded_Cleanup;

        void handler(object sender, RoutedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                new Action(() =>
                {
                    FocusElement(returnElement);
                }),
                DispatcherPriority.Input);
        }

        triggerElement.Click += handler;
        RegisteredClickHandlers[triggerElement] = handler;
    }

    private static void Element_Unloaded_Cleanup(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            fe.Unloaded -= Element_Unloaded_Cleanup;
            UnregisterKeyDownInternal(fe);

            if (fe is Button btn)
                UnregisterClickInternal(btn);
        }
    }

    private static void UnregisterKeyDownInternal(UIElement element)
    {
        if (RegisteredKeyDownHandlers.TryGetValue(element, out var handler))
        {
            element.PreviewKeyDown -= handler;
            RegisteredKeyDownHandlers.Remove(element);
        }
    }

    private static void UnregisterClickInternal(Button button)
    {
        if (RegisteredClickHandlers.TryGetValue(button, out var handler))
        {
            button.Click -= handler;
            RegisteredClickHandlers.Remove(button);
        }
    }

    private static void HandleTextBoxPointerKeys(KeyEventArgs e, TextBox tb, int currentIdx, bool shift, List<UIElement> focusOrder, out UIElement? nextElement)
    {
        nextElement = null;

        switch (e.Key)
        {
            case Key.Right:
                if (shift)
                    nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: false);
                else if (tb.SelectionLength == tb.Text.Length || tb.CaretIndex == tb.Text.Length)
                    nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: true);
                break;

            case Key.Left:
                if (shift)
                    nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: true);
                else if (tb.SelectionLength == tb.Text.Length || tb.CaretIndex == 0)
                    nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: false);
                break;

            case Key.Down:
                nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: !shift);
                break;

            case Key.Up:
                nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: shift);
                break;
        }
    }

    private static void HandleComboBoxPointerKeys(KeyEventArgs e, int currentIdx, bool shift, List<UIElement> focusOrder, out UIElement? nextElement)
    {
        nextElement = null;

        switch (e.Key)
        {
            case Key.Down:
            case Key.Up:
                break;

            case Key.Right:
                nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: !shift);
                break;

            case Key.Left:
                nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: shift);
                break;
        }
    }

    private static void HandleGeneralPointerKeys(KeyEventArgs e, int currentIdx, bool shift, List<UIElement> focusOrder, out UIElement? nextElement)
    {
        nextElement = null;

        switch (e.Key)
        {
            case Key.Down:
            case Key.Right:
                nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: !shift);
                break;

            case Key.Up:
            case Key.Left:
                nextElement = GetNextFocusableElement(focusOrder, currentIdx, isForward: shift);
                break;
        }
    }

    private static UIElement? GetNextFocusableElement(List<UIElement> focusOrder, int currentIdx, bool isForward)
    {
        if (isForward)
        {
            for (int i = currentIdx + 1; i < focusOrder.Count; i++)
            {
                if (IsElementFocusable(focusOrder[i]))
                {
                    return focusOrder[i];
                }
            }
        }
        else
        {
            for (int i = currentIdx - 1; i >= 0; i--)
            {
                if (IsElementFocusable(focusOrder[i]))
                {
                    return focusOrder[i];
                }
            }
        }
        return null;
    }

    private static bool IsElementFocusable(UIElement element)
    {
        if (element.Visibility != Visibility.Visible || !element.IsEnabled)
            return false;

        return element switch
        {
            TextBox tb => tb.IsVisible && tb.IsEnabled && tb.IsTabStop,
            ComboBox cb => cb.IsVisible && cb.IsEnabled,
            Button btn => btn.IsVisible && btn.IsEnabled && btn.IsTabStop,
            _ => element.IsVisible && element.IsEnabled,
        };
    }

    private static void FocusElement(UIElement element)
    {
        element.Focus();

        if (element is TextBox tb)
        {
            tb.SelectAll();
            return;
        }

        if (element is ComboBox cb && cb.IsEditable)
            if (cb.Template.FindName("PART_EditableTextBox", cb) is TextBox innerTextBox)
                innerTextBox.SelectAll();
    }
}
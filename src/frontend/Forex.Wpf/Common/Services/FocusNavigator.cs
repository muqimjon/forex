namespace Forex.Wpf.Common.Services;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class FocusNavigator
{
    public static void AttachEnterNavigation(List<UIElement> focusOrder)
    {
        if (focusOrder is null || focusOrder.Count == 0)
            return;

        FocusElement(focusOrder[0]);

        foreach (var control in focusOrder)
        {
            control.PreviewKeyDown += (s, e) =>
            {
                bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

                int idx = focusOrder.IndexOf(control);
                UIElement? nextElement = null;

                switch (e.Key)
                {
                    case Key.Enter:
                    case Key.Tab:
                        if (shift)
                        {
                            // Orqaga
                            for (int i = idx - 1; i >= 0; i--)
                            {
                                if (IsElementFocusable(focusOrder[i]))
                                {
                                    nextElement = focusOrder[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Oldinga
                            for (int i = idx + 1; i < focusOrder.Count; i++)
                            {
                                if (IsElementFocusable(focusOrder[i]))
                                {
                                    nextElement = focusOrder[i];
                                    break;
                                }
                            }

                            // Oxirgi element va Button bo'lsa, click qilamiz
                            if (nextElement is null && control is Button btn)
                            {
                                btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                e.Handled = true;
                                return;
                            }
                        }

                        if (nextElement is not null)
                        {
                            FocusElement(nextElement);
                            e.Handled = true;
                        }
                        break;

                    case Key.Up:
                        // Orqaga arrow
                        for (int i = idx - 1; i >= 0; i--)
                        {
                            if (IsElementFocusable(focusOrder[i]))
                            {
                                nextElement = focusOrder[i];
                                break;
                            }
                        }
                        if (nextElement is not null)
                        {
                            FocusElement(nextElement);
                            e.Handled = true;
                        }
                        break;

                    case Key.Down:
                        // Oldinga arrow
                        for (int i = idx + 1; i < focusOrder.Count; i++)
                        {
                            if (IsElementFocusable(focusOrder[i]))
                            {
                                nextElement = focusOrder[i];
                                break;
                            }
                        }
                        if (nextElement is not null)
                        {
                            FocusElement(nextElement);
                            e.Handled = true;
                        }
                        break;
                }
            };
        }
    }

    private static bool IsElementFocusable(UIElement element)
    {
        if (element.Visibility != Visibility.Visible || !element.IsEnabled)
            return false;

        return element switch
        {
            TextBox tb => tb.IsVisible && tb.IsEnabled && tb.IsTabStop,
            ComboBox cb => cb.IsVisible && cb.IsEnabled && cb.IsTabStop,
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
        {
            if (cb.Template.FindName("PART_EditableTextBox", cb) is TextBox innerTextBox)
            {
                innerTextBox.SelectAll();
            }
        }
    }
}

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
                if (e.Key != Key.Enter && e.Key != Key.Tab)
                    return;

                bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

                // Hozirgi elementning indexini aniqlash
                int idx = focusOrder.IndexOf(control);

                UIElement? nextElement = null;

                if (shift)
                {
                    // Orqaga yurish → faqat Visible elementni topish
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
                    // Oldinga yurish → faqat Visible elementni topish
                    for (int i = idx + 1; i < focusOrder.Count; i++)
                    {
                        if (IsElementFocusable(focusOrder[i]))
                        {
                            nextElement = focusOrder[i];
                            break;
                        }
                    }
                }

                if (nextElement is not null)
                {
                    FocusElement(nextElement);
                    e.Handled = true;
                }
            };
        }
    }

    private static bool IsElementFocusable(UIElement element)
    {
        if (element.Visibility != Visibility.Visible || !element.IsEnabled)
            return false;

        switch (element)
        {
            case TextBox tb:
                return tb.IsVisible && tb.IsEnabled && tb.IsTabStop;
            case ComboBox cb:
                return cb.IsVisible && cb.IsEnabled && cb.IsTabStop;
            case Button btn:
                return btn.IsVisible && btn.IsEnabled && btn.IsTabStop;
            default:
                return element.IsVisible && element.IsEnabled;
        }
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

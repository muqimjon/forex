namespace Forex.Wpf.Services;

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

                int idx = focusOrder.IndexOf(control);
                bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

                if (shift && idx > 0)
                    FocusElement(focusOrder[idx - 1]);
                else if (!shift && idx < focusOrder.Count - 1)
                    FocusElement(focusOrder[idx + 1]);

                e.Handled = true;
            };
        }
    }

    // cursorni element ichidagi content oxiriga joylash
    private static void FocusElement(UIElement element)
    {
        element.Focus();

        // TextBox bo‘lsa
        if (element is TextBox tb)
        {
            tb.CaretIndex = tb.Text?.Length ?? 0;
            return;
        }

        // Editable ComboBox bo‘lsa
        if (element is ComboBox cb && cb.IsEditable)
        {
            if (cb.Template.FindName("PART_EditableTextBox", cb) is TextBox innerTextBox)
            {
                innerTextBox.CaretIndex = innerTextBox.Text?.Length ?? 0;
            }
        }

        // Agar boshqa custom control bo‘lsa, davom etishimiz mumkin...
    }
}

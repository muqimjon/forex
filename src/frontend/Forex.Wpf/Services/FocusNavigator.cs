namespace Forex.Wpf.Services;

using System.Windows;
using System.Windows.Input;

public static class FocusNavigator
{
    public static void AttachEnterNavigation(List<UIElement> focusOrder)
    {
        if (focusOrder is null || focusOrder.Count == 0)
            return;

        focusOrder[0].Focus();

        foreach (var control in focusOrder)
        {
            control.PreviewKeyDown += (s, e) =>
            {
                if (e.Key != Key.Enter && e.Key != Key.Tab)
                    return;

                int idx = focusOrder.IndexOf(control);
                bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

                if (shift)
                {
                    if (idx > 0)
                        focusOrder[idx - 1].Focus();
                }
                else
                {
                    if (idx < focusOrder.Count - 1)
                        focusOrder[idx + 1].Focus();
                }

                e.Handled = true;
            };
        }
    }
}
namespace Forex.Wpf.Services;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

public static class FocusNavigator
{
    public static void AttachEnterNavigation(List<UIElement> focusOrder)
    {
        if (focusOrder is null || focusOrder.Count == 0)
            return;

        foreach (var control in focusOrder)
        {
            control.PreviewKeyDown += (_, e) =>
            {
                if (e.Key != Key.Enter) return;

                int index = focusOrder.IndexOf(control);
                if (index >= 0 && index < focusOrder.Count - 1)
                {
                    focusOrder[index + 1].Focus();
                    e.Handled = true;
                }
            };
        }
    }
}

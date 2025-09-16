namespace Forex.Wpf.Common.Extensions;

using System.Windows;
using System.Windows.Media;

public static class VisualTreeHelperExtensions
{
    public static T? FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent is null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is FrameworkElement fe && fe.Name == childName && child is T typedChild)
                return typedChild;

            var result = child.FindChild<T>(childName);
            if (result is not null)
                return result;
        }

        return null;
    }

    public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);

        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);

        return (parent as T)!;
    }
}

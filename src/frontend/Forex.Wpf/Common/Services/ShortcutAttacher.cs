namespace Forex.Wpf.Common.Services;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public static class ShortcutAttacher
{
    private class ShortcutRegistration
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public Action Action { get; set; } = null!;
        public string? TooltipText { get; set; }
    }

    private static readonly Dictionary<FrameworkElement, List<KeyEventHandler>> ActiveHandlers = [];
    private static readonly Dictionary<FrameworkElement, List<ShortcutRegistration>> PendingRegistrations = [];
    private static readonly HashSet<FrameworkElement> InitializedViews = [];

    private static T? FindVisualParent<T>(DependencyObject element) where T : DependencyObject
    {
        while (element is not null)
        {
            if (element is T parent)
                return parent;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private static FrameworkElement? FindRootView(FrameworkElement element)
    {
        var view = FindVisualParent<UserControl>(element) as FrameworkElement
                   ?? FindVisualParent<Page>(element) as FrameworkElement
                   ?? FindVisualParent<Window>(element) as FrameworkElement;

        if (view is null)
        {
            DependencyObject current = element;
            while (current is not null)
            {
                if (current is UserControl or Page or Window)
                {
                    view = current as FrameworkElement;
                    break;
                }
                current = LogicalTreeHelper.GetParent(current);
            }
        }

        return view;
    }

    private static void SetupViewLifecycleHandlers(FrameworkElement view)
    {
        if (InitializedViews.Contains(view))
            return;

        InitializedViews.Add(view);

        void loadedHandler(object sender, RoutedEventArgs e)
        {
            ActivatePendingShortcuts(view);
        }

        void unloadedHandler(object sender, RoutedEventArgs e)
        {
            DeactivateShortcuts(view);
        }

        view.Loaded += loadedHandler;
        view.Unloaded += unloadedHandler;

        if (view is Page page)
        {
            void navigationHandler(object sender, System.Windows.Navigation.NavigationEventArgs e)
            {
                if (e.Content != page)
                {
                    DeactivateShortcuts(page);
                }
            }

            page.Loaded += (s, e) =>
            {
                if (page.NavigationService is not null)
                {
                    page.NavigationService.Navigated -= navigationHandler;
                    page.NavigationService.Navigated += navigationHandler;
                }
            };

            page.Unloaded += (s, e) =>
            {
                if (page.NavigationService is not null)
                {
                    page.NavigationService.Navigated -= navigationHandler;
                }
            };
        }
    }

    private static void ActivatePendingShortcuts(FrameworkElement view)
    {
        DeactivateShortcuts(view);

        if (!PendingRegistrations.TryGetValue(view, out var registrations))
            return;

        foreach (var reg in registrations)
        {
            void handler(object sender, KeyEventArgs e)
            {
                var currentModifiers = Keyboard.Modifiers;
                bool modifiersMatch = reg.Modifiers == ModifierKeys.None
                    ? currentModifiers == ModifierKeys.None
                    : (currentModifiers & reg.Modifiers) == reg.Modifiers;

                if (e.Key == reg.Key && modifiersMatch)
                {
                    reg.Action.Invoke();
                    e.Handled = true;
                }
            }

            view.PreviewKeyDown += handler;

            if (!ActiveHandlers.TryGetValue(view, out List<KeyEventHandler>? value))
            {
                value = [];
                ActiveHandlers[view] = value;
            }

            value.Add(handler);
        }
    }

    private static void DeactivateShortcuts(FrameworkElement view)
    {
        if (ActiveHandlers.TryGetValue(view, out var handlers))
        {
            foreach (var handler in handlers)
            {
                view.PreviewKeyDown -= handler;
            }
            ActiveHandlers.Remove(view);
        }
    }

    public static void RegisterShortcut(FrameworkElement targetElement, Key key, Action targetAction, ModifierKeys modifiers = ModifierKeys.None)
    {
        if (targetElement is null || targetAction is null)
            return;

        FrameworkElement? view = null;

        if (targetElement.IsLoaded)
        {
            view = FindRootView(targetElement);
        }

        if (view is null)
        {
            void delayedRegistration(object? s, RoutedEventArgs e)
            {
                targetElement.Loaded -= delayedRegistration;
                RegisterShortcut(targetElement, key, targetAction, modifiers);
            }
            targetElement.Loaded += delayedRegistration;
            return;
        }

        SetupViewLifecycleHandlers(view);

        if (!PendingRegistrations.TryGetValue(view, out List<ShortcutRegistration>? value))
        {
            value = [];
            PendingRegistrations[view] = value;
        }

        var exists = value.Any(r =>
            r.Key == key && r.Modifiers == modifiers);

        if (!exists)
        {
            value.Add(new ShortcutRegistration
            {
                Key = key,
                Modifiers = modifiers,
                Action = targetAction
            });
        }

        if (view.IsLoaded)
        {
            ActivatePendingShortcuts(view);
        }
    }

    public static void RegisterShortcut(Button targetButton, Key key, ModifierKeys modifiers = ModifierKeys.None)
    {
        if (targetButton is null)
            return;

        string shortcutString = KeyCombinationToString(key, modifiers);
        UpdateTooltip(targetButton, shortcutString);

        void buttonAction()
        {
            if (!targetButton.IsEnabled || targetButton.Visibility != Visibility.Visible)
                return;

            if (targetButton.Command is not null && targetButton.Command.CanExecute(targetButton.CommandParameter))
                targetButton.Command.Execute(targetButton.CommandParameter);
            else
                targetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        RegisterShortcut(targetButton, key, buttonAction, modifiers);
    }

    private static string KeyCombinationToString(Key key, ModifierKeys modifiers)
    {
        var parts = new List<string>();

        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");

        if (key != Key.None)
        {
            string keyName = key switch
            {
                Key.Return => "Enter",
                Key.Delete => "Del",
                Key.Escape => "Esc",
                Key.Tab => "Tab",
                Key.Capital => "Caps Lock",
                Key.Space => "Space",
                Key.LWin or Key.RWin => "Win",
                Key.Add => "Num +",
                Key.Subtract => "Num -",
                Key.Multiply => "Num *",
                Key.Divide => "Num /",
                _ => key.ToString()
            };
            parts.Add(keyName);
        }

        string shortcut = string.Join(" + ", parts);
        return $"({shortcut})";
    }

    private static void UpdateTooltip(Button targetButton, string shortcutString)
    {
        object currentTooltip = targetButton.ToolTip;

        if (currentTooltip is string currentText && !string.IsNullOrWhiteSpace(currentText))
        {
            if (!currentText.Contains(shortcutString))
                targetButton.ToolTip = $"{currentText} {shortcutString}";
        }
        else if (currentTooltip is null)
            targetButton.ToolTip = shortcutString;
    }

    public static void ClearAll()
    {
        foreach (var view in ActiveHandlers.Keys.ToList())
        {
            DeactivateShortcuts(view);
        }
        PendingRegistrations.Clear();
        InitializedViews.Clear();
    }
}
namespace Forex.Wpf.Common.Services;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

public static class FocusNavigator
{
    #region Private Classes

    private class FocusContext
    {
        public List<UIElement> FocusOrder { get; set; } = [];
        public Dictionary<UIElement, KeyEventHandler> KeyDownHandlers { get; set; } = [];
        public Dictionary<Button, RoutedEventHandler> ClickHandlers { get; set; } = [];
        public FrameworkElement? RootView { get; set; }
    }

    #endregion

    #region Private Fields

    private static readonly Dictionary<FrameworkElement, FocusContext> ViewContexts = [];
    private static readonly Lock _lock = new();

    #endregion

    #region Public Methods

    public static void RegisterElements(List<UIElement> focusOrder)
    {
        if (focusOrder is null || focusOrder.Count == 0)
            return;

        var rootView = FindRootView(focusOrder[0]);
        if (rootView is null)
        {
            if (focusOrder[0] is FrameworkElement fe && !fe.IsLoaded)
            {
                void delayedRegistration(object? s, RoutedEventArgs e)
                {
                    fe.Loaded -= delayedRegistration;
                    RegisterElements(focusOrder);
                }
                fe.Loaded += delayedRegistration;
            }
            return;
        }

        lock (_lock)
        {
            if (ViewContexts.TryGetValue(rootView, out var oldContext))
            {
                CleanupContext(oldContext);
            }

            var context = new FocusContext
            {
                FocusOrder = [.. focusOrder],
                RootView = rootView
            };

            ViewContexts[rootView] = context;

            SetupViewLifecycle(rootView);
            RegisterKeyHandlers(context);
            FocusElement(focusOrder[0]);
        }
    }

    public static void SetFocusRedirect(Button triggerElement, UIElement returnElement)
    {
        if (triggerElement is null || returnElement is null)
            return;

        var rootView = FindRootView(triggerElement);
        if (rootView is null)
        {
            if (!triggerElement.IsLoaded)
            {
                void delayedRegistration(object? s, RoutedEventArgs e)
                {
                    triggerElement.Loaded -= delayedRegistration;
                    SetFocusRedirect(triggerElement, returnElement);
                }
                triggerElement.Loaded += delayedRegistration;
            }
            return;
        }

        lock (_lock)
        {
            if (!ViewContexts.TryGetValue(rootView, out var context))
            {
                context = new FocusContext { RootView = rootView };
                ViewContexts[rootView] = context;
                SetupViewLifecycle(rootView);
            }

            if (context.ClickHandlers.TryGetValue(triggerElement, out var oldHandler))
            {
                triggerElement.Click -= oldHandler;
            }

            void handler(object sender, RoutedEventArgs e)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    new Action(() => FocusElement(returnElement)),
                    DispatcherPriority.Input);
            }

            triggerElement.Click += handler;
            context.ClickHandlers[triggerElement] = handler;
        }
    }

    public static void UnregisterView(FrameworkElement view)
    {
        if (view is null)
            return;

        lock (_lock)
        {
            if (ViewContexts.TryGetValue(view, out var context))
            {
                CleanupContext(context);
                ViewContexts.Remove(view);
            }
        }
    }

    #endregion

    #region Private Helper Methods

    private static FrameworkElement? FindRootView(DependencyObject element)
    {
        var view = FindVisualParent<UserControl>(element) as FrameworkElement
                   ?? FindVisualParent<Page>(element) as FrameworkElement
                   ?? FindVisualParent<Window>(element);

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

    private static void SetupViewLifecycle(FrameworkElement view)
    {
        void unloadedHandler(object sender, RoutedEventArgs e)
        {
            view.Unloaded -= unloadedHandler;
            UnregisterView(view);
        }

        view.Unloaded += unloadedHandler;

        if (view is Page page)
        {
            void navigationHandler(object sender, System.Windows.Navigation.NavigationEventArgs e)
            {
                if (e.Content != page)
                {
                    UnregisterView(page);
                }
            }

            void loadedHandler(object sender, RoutedEventArgs e)
            {
                page.Loaded -= loadedHandler;
                if (page.NavigationService is not null)
                {
                    page.NavigationService.Navigated -= navigationHandler;
                    page.NavigationService.Navigated += navigationHandler;
                }
            }

            page.Loaded += loadedHandler;

            void cleanupNavigationHandler(object sender, RoutedEventArgs e)
            {
                page.Unloaded -= cleanupNavigationHandler;
                if (page.NavigationService is not null)
                {
                    page.NavigationService.Navigated -= navigationHandler;
                }
            }

            page.Unloaded += cleanupNavigationHandler;
        }
    }

    private static void RegisterKeyHandlers(FocusContext context)
    {
        foreach (var element in context.FocusOrder)
        {
            void handler(object s, KeyEventArgs e)
            {
                HandleKeyDown(element, e, context);
            }

            element.PreviewKeyDown += handler;
            context.KeyDownHandlers[element] = handler;
        }
    }

    private static void HandleKeyDown(UIElement element, KeyEventArgs e, FocusContext context)
    {
        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        int currentIdx = context.FocusOrder.IndexOf(element);

        if (currentIdx == -1)
            return;

        UIElement? nextElement = null;

        if (e.Key == Key.Enter || e.Key == Key.Tab)
        {
            if (element is Button btn && e.Key == Key.Enter && !shift)
            {
                btn.Command?.Execute(btn.CommandParameter);
                btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
                return;
            }

            nextElement = shift
                ? GetNextFocusableElement(context.FocusOrder, currentIdx, isForward: false)
                : GetNextFocusableElement(context.FocusOrder, currentIdx, isForward: true);
        }
        else if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            nextElement = element switch
            {
                TextBox tb => HandleTextBoxNavigation(e, tb, currentIdx, shift, context.FocusOrder),
                ComboBox => HandleComboBoxNavigation(e, currentIdx, shift, context.FocusOrder),
                _ => HandleGeneralNavigation(e, currentIdx, shift, context.FocusOrder)
            };
        }

        if (nextElement is not null)
        {
            FocusElement(nextElement);
            e.Handled = true;
        }
    }

    private static UIElement? HandleTextBoxNavigation(KeyEventArgs e, TextBox tb, int currentIdx, bool shift, List<UIElement> focusOrder)
    {
        return e.Key switch
        {
            Key.Right when shift => GetNextFocusableElement(focusOrder, currentIdx, false),
            Key.Right when tb.SelectionLength == tb.Text.Length || tb.CaretIndex == tb.Text.Length
                => GetNextFocusableElement(focusOrder, currentIdx, true),

            Key.Left when shift => GetNextFocusableElement(focusOrder, currentIdx, true),
            Key.Left when tb.SelectionLength == tb.Text.Length || tb.CaretIndex == 0
                => GetNextFocusableElement(focusOrder, currentIdx, false),

            Key.Down => GetNextFocusableElement(focusOrder, currentIdx, !shift),
            Key.Up => GetNextFocusableElement(focusOrder, currentIdx, shift),

            _ => null
        };
    }

    private static UIElement? HandleComboBoxNavigation(KeyEventArgs e, int currentIdx, bool shift, List<UIElement> focusOrder)
    {
        return e.Key switch
        {
            Key.Down or Key.Up => null,
            Key.Right => GetNextFocusableElement(focusOrder, currentIdx, !shift),
            Key.Left => GetNextFocusableElement(focusOrder, currentIdx, shift),
            _ => null
        };
    }

    private static UIElement? HandleGeneralNavigation(KeyEventArgs e, int currentIdx, bool shift, List<UIElement> focusOrder)
    {
        return e.Key switch
        {
            Key.Down or Key.Right => GetNextFocusableElement(focusOrder, currentIdx, !shift),
            Key.Up or Key.Left => GetNextFocusableElement(focusOrder, currentIdx, shift),
            _ => null
        };
    }

    private static UIElement? GetNextFocusableElement(List<UIElement> focusOrder, int currentIdx, bool isForward)
    {
        int count = focusOrder.Count;
        int step = isForward ? 1 : -1;

        for (int i = 1; i <= count; i++)
        {
            int nextIdx = (currentIdx + i * step + count) % count;

            if (IsElementFocusable(focusOrder[nextIdx]) && nextIdx != currentIdx)
            {
                return focusOrder[nextIdx];
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
            TextBox tb => tb.IsTabStop,
            ComboBox => true,
            Button btn => btn.IsTabStop,
            _ => true
        };
    }

    private static void FocusElement(UIElement element)
    {
        element.Focus();

        switch (element)
        {
            case TextBox tb: tb.SelectAll(); break;

            case ComboBox { IsEditable: true } cb:
                if (cb.Template.FindName("PART_EditableTextBox", cb) is TextBox innerTextBox)
                    innerTextBox.SelectAll();
                break;
        }
    }

    private static void CleanupContext(FocusContext context)
    {
        foreach (var kvp in context.KeyDownHandlers)
            kvp.Key.PreviewKeyDown -= kvp.Value;
        context.KeyDownHandlers.Clear();

        foreach (var kvp in context.ClickHandlers)
            kvp.Key.Click -= kvp.Value;
        context.ClickHandlers.Clear();

        context.FocusOrder.Clear();
    }

    #endregion
}
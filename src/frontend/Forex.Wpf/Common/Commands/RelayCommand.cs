namespace Forex.Wpf.Common.Commands;

using System.Windows.Input;

// Non-generic
public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
{
    private readonly Action<object?> execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public RelayCommand(Action execute) : this(_ => execute(), null) { }

    public RelayCommand(Action execute, Func<bool> canExecute)
        : this(_ => execute(), _ => canExecute()) { }

    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

// Generic
public class RelayCommand<T>(Action<T?> execute, Predicate<T?>? canExecute = null) : ICommand
{
    private readonly Action<T?> execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke(ConvertParameter(parameter)) ?? true;
    }

    public void Execute(object? parameter)
    {
        execute(ConvertParameter(parameter));
    }

    private static T? ConvertParameter(object? parameter)
    {
        if (parameter is null)
            return default;

        return (T)parameter;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

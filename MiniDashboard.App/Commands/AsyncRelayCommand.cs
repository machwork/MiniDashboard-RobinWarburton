using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace MiniDashboard.App.Commands;

/// <summary>
/// Async-aware ICommand implementation that accepts a Func<object?, Task>.
/// It prevents re-entrancy while the command is executing and raises CanExecuteChanged.
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute == null || _canExecute(parameter));

    public async void Execute(object? parameter)
    {
        await ExecuteAsync(parameter).ConfigureAwait(false);
    }

    public async Task ExecuteAsync(object? parameter)
    {
        if (!CanExecute(parameter)) return;

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _execute(parameter).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Surface exception on UI dispatcher so it is observable during debugging/runtime
            try
            {
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                {
                    // Fire-and-forget: intentionally not awaited to avoid blocking
                    _ = dispatcher.BeginInvoke(new Action(() => throw ex));
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                // Swallow to avoid secondary exceptions if dispatcher unavailable
            }
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    private void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

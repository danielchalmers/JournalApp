using System.Runtime.CompilerServices;
using MudBlazor;

namespace JournalApp;

public sealed class KeyEventService(ILogger<KeyEventService> logger)
{
    private readonly Stack<Action> _backButtonActions = [];

    public int CurrentDepth => _backButtonActions.Count;

    public void ResetStack() => _backButtonActions.Clear();

    public void Entered(Action backButtonAction, [CallerFilePath] string callerFilePath = "")
    {
        lock (_backButtonActions)
        {
            logger.LogDebug($"Entering at {CurrentDepth} depth <{callerFilePath}>");

            _backButtonActions.Push(backButtonAction);
        }
    }

    public void Exited([CallerFilePath] string callerFilePath = "")
    {
        lock (_backButtonActions)
        {
            logger.LogDebug($"Exiting at {CurrentDepth} depth <{callerFilePath}>");

            if (CurrentDepth != 0)
                _backButtonActions.Pop();
        }
    }

    public void CancelDialog(IMudDialogInstance dialogInstance)
    {
        Exited();
        dialogInstance.Cancel();
    }

    public void CloseDialog<T>(IMudDialogInstance dialogInstance, T returnValue)
    {
        Exited();
        dialogInstance.Close(returnValue);
    }

    internal bool OnBackButtonPressed()
    {
        logger.LogInformation("Back button pressed");

        var anyActions = CurrentDepth != 0;
        if (anyActions)
            _backButtonActions.Peek().Invoke();

        // Consider the event handled if there were any subscriptions.
        // ex: a dialog used the event to close itself; or none were open and therefore the platform decides what to do.
        return anyActions;
    }
}

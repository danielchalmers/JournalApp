﻿using System.Runtime.CompilerServices;
using MudBlazor;

namespace JournalApp;

public sealed class KeyEventService(ILogger<KeyEventService> logger)
{
    private readonly Stack<Action> _backButtonActions = [];

    public int CurrentDepth => _backButtonActions.Count;

    public void ResetStack() => _backButtonActions.Clear();

    public void Entered(Action backButtonAction, [CallerFilePath] string callerFilePath = "")
    {
        logger.LogDebug($"Entering at {CurrentDepth} depth <{callerFilePath}>");

        lock (_backButtonActions)
        {
            _backButtonActions.Push(backButtonAction);
        }
    }

    public void Exited([CallerFilePath] string callerFilePath = "")
    {
        logger.LogDebug($"Exiting at {CurrentDepth} depth <{callerFilePath}>");

        lock (_backButtonActions)
        {
            if (CurrentDepth != 0)
                _backButtonActions.Pop();
        }
    }

    public void CancelDialog(MudDialogInstance dialogInstance)
    {
        Exited();
        dialogInstance.Cancel();
    }

    public void CloseDialog<T>(MudDialogInstance dialogInstance, T returnValue)
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

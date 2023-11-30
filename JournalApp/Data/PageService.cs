using MudBlazor;

namespace JournalApp;

public class PageService(ILogger<PageService> logger)
{
    private readonly Stack<Action> _backButtonPressedActions = [];

    public void EnteredPage(Action backButtonAction)
    {
        logger.LogDebug("Entered page");

        lock (_backButtonPressedActions)
        {
            _backButtonPressedActions.Push(backButtonAction);
        }
    }

    public void ExitedPage()
    {
        logger.LogDebug("Exited page");

        lock (_backButtonPressedActions)
        {
            if (_backButtonPressedActions.Count != 0)
                _backButtonPressedActions.Pop();
        }
    }

    public void CancelDialog(MudDialogInstance dialogInstance)
    {
        ExitedPage();
        dialogInstance.Cancel();
    }

    public void CloseDialog<T>(MudDialogInstance dialogInstance, T returnValue)
    {
        ExitedPage();
        dialogInstance.Close(returnValue);
    }

    internal bool OnBackButtonPressed()
    {
        logger.LogInformation("Back button pressed");

        var anyActions = _backButtonPressedActions.Count != 0;
        if (anyActions)
            _backButtonPressedActions.First().Invoke();

        // Consider the event handled if there were any subscriptions.
        // ex: a dialog used the event to close itself; or none were open and therefore the platform decides what to do.
        return anyActions;
    }
}

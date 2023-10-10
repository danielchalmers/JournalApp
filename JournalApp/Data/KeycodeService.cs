namespace JournalApp;

public class KeycodeService
{
    private readonly List<Action> _backButtonPressedActions = [];

    public void SubscribeOnceToBackButtonPressed(Action action)
    {
        lock (_backButtonPressedActions)
        {
            _backButtonPressedActions.Add(action);
        }
    }

    internal bool OnBackButtonPressed()
    {
        // Copy to temp array before using to avoid deadlocks during invocation.
        Action[] temp;
        lock (_backButtonPressedActions)
        {
            temp = _backButtonPressedActions.ToArray();
            _backButtonPressedActions.Clear();
        }

        // Invoke every subscription.
        foreach (var action in temp)
        {
            action.Invoke();
        }

        // Consider the event handled if any subscriptions were invoked.
        // ex: a dialog used the event to close itself; or none were open and therefore the platform decides what to do.
        return temp.Length != 0;
    }
}

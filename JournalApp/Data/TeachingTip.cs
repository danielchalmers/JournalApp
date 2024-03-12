using MudBlazor;

namespace JournalApp;

public static class TeachingTip
{
    public static void ShowTeachingTip(this ISnackbar snackbar, string name, string text, bool oneTime = false)
    {
        if (Preferences.Get($"tip_{name}", string.Empty) == "seen")
            return;

        snackbar.Add(text, Severity.Info, key: name);

        if (oneTime)
            Preferences.Set($"tip_{name}", "seen");
    }

    public static void ActionTaken(this ISnackbar snackbar, string name)
    {
        snackbar.RemoveByKey(name);

        Preferences.Set($"tip_{name}", "seen");
    }
}

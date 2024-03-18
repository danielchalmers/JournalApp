using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace JournalApp;

public partial class JaPage : ComponentBase
{
    [Inject]
    protected KeyEventService KeyEventService { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected IDbContextFactory<AppDbContext> DbFactory { get; set; }

    [Inject]
    protected IDialogService DialogService { get; set; }

    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    [Inject]
    protected IScrollManager ScrollManager { get; set; }

    [Inject]
    protected ISnackbar Snackbar { get; set; }

    [Inject]
    protected IPreferences Preferences { get; set; }

    [Inject]
    protected AppThemeService AppThemeService { get; set; }

    public void ShowTeachingTip(string name, string text, bool oneTime = false)
    {

        if (Preferences.Get($"tip_{name}", string.Empty) == "seen")
            return;

        Snackbar.Add(text, Severity.Info, key: name);

        if (oneTime)
            Preferences.Set($"tip_{name}", "seen");
    }

    public void ActionTaken(string name)
    {
        Snackbar.RemoveByKey(name);

        Preferences.Set($"tip_{name}", "seen");
    }
}

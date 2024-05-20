using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace JournalApp;

public partial class JaPage : ComponentBase, IDisposable
{
    IDisposable _locationChangingRegistration;

    [Inject]
    protected KeyEventService KeyEventService { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected IDialogService DialogService { get; set; }

    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    [Inject]
    protected IScrollManager ScrollManager { get; set; }

    [Inject]
    protected ISnackbar Snackbar { get; set; }

    [Inject]
    protected PreferenceService PreferenceService { get; set; }

    /// <summary>
    /// Indicates whether the user is in the process of leaving the current page.
    /// </summary>
    protected bool IsLeaving { get; set; }

    /// <summary>
    /// Indicates whether the page has been disposed.
    /// </summary>
    protected bool IsDisposed { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Subscribe to window events if the window is available (not in tests).
        if (App.Window is not null)
        {
            App.Window.Deactivated += OnWindowDeactivatedOrDestroying;
            App.Window.Destroying += OnWindowDeactivatedOrDestroying;
            App.Window.Resumed += OnWindowResumed;
        }

        // Register the handler for location changing event.
        _locationChangingRegistration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);
    }

    /// <summary>
    /// Handles the event when the main window is deactivated or in the process of being destroyed.<br/>
    /// This will happen when you tab out on desktop or when you switch apps on mobile.
    /// </summary>
    protected virtual void OnWindowDeactivatedOrDestroying(object sender, EventArgs e)
    {
        Debug.Assert(!IsDisposed);
        SaveState();
    }

    /// <summary>
    /// Handles the event when the main window is deactivated or in the process of being destroyed.<br/>
    /// This will happen when you tab out on desktop or when you switch apps on mobile.
    /// </summary>
    protected virtual void OnWindowResumed(object sender, EventArgs e)
    {
        Debug.Assert(!IsDisposed);
    }

    /// <summary>
    /// Saves the state of the page.<br/>
    /// Called when the page is navigated away or the app is switched.
    /// </summary>
    protected virtual void SaveState()
    {
        Debug.Assert(!IsDisposed);
        // TODO: Async support.
    }

    /// <summary>
    /// Handles the event when the location is changing.<br/>
    /// Sets <see cref="IsLeaving"/> to <c>true</c>.
    /// </summary>
    protected virtual ValueTask OnLocationChanging(LocationChangingContext e)
    {
        IsLeaving = true;
        SaveState();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Shows a teaching tip using the <see cref="Snackbar"/> service.
    /// </summary>
    /// <param name="name">The name of the tip.</param>
    /// <param name="text">The text of the tip.</param>
    /// <param name="oneTime">Indicates if the tip should only be shown once instead of every time until an action is taken.</param>
    public void ShowTeachingTip(string name, string text, bool oneTime = false)
    {
        if (PreferenceService.Get($"tip_{name}", string.Empty) == "seen")
            return;

        Snackbar.Add(text, Severity.Info, key: name);

        if (oneTime)
            PreferenceService.Set($"tip_{name}", "seen");
    }

    /// <summary>
    /// Marks an action as taken and removes the corresponding Snackbar message.
    /// </summary>
    /// <param name="name">The name of the tip.</param>
    public void ActionTaken(string name)
    {
        Snackbar.RemoveByKey(name);

        PreferenceService.Set($"tip_{name}", "seen");
    }

    /// <summary>
    /// Disposes resources used by the page.
    /// </summary>
    /// <param name="disposing">Indicates whether the method was called from the public Dispose method.</param>
    protected virtual void Dispose(bool disposing)
    {
        // Don't dispose the page more than once.
        if (IsDisposed)
        {
            return;
        }

        // Save the state if we're not just navigating to another page because that would be handled by LocationChangingContext.
        if (!IsLeaving)
        {
            SaveState();
        }

        if (disposing)
        {
            // TODO: dispose managed state (managed objects)

            // Unsubscribe from window events if the window is available (not in tests).
            if (App.Window is not null)
            {
                App.Window.Deactivated -= OnWindowDeactivatedOrDestroying;
                App.Window.Destroying -= OnWindowDeactivatedOrDestroying;
                App.Window.Resumed -= OnWindowResumed;
            }

            _locationChangingRegistration?.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer.
        // TODO: set large fields to null.
        IsDisposed = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // https://github.com/dotnet/aspnetcore/issues/53863.
    protected override bool ShouldRender() => !IsLeaving;
}

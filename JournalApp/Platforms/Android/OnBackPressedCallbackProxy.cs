using AndroidX.Activity;

namespace JournalApp.Platforms.Android;

internal class OnBackPressedCallbackProxy(Action onBackPressed) : OnBackPressedCallback(true)
{
    public override void HandleOnBackPressed()
    {
        var navigation = Application.Current?.MainPage?.Navigation;
        if (navigation is null || navigation.NavigationStack.Count > 1 || navigation.ModalStack.Count > 0)
            return;

        onBackPressed?.Invoke();
    }
}
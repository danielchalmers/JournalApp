using Android.App;
using Android.Content.PM;
using Android.OS;
using JournalApp.Platforms.Android;

namespace JournalApp;

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleInstance, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        // The WebView's selection handles, magnifier and caret are native chrome tinted from the activity theme, and the theme is read once when the view is created.
        // The theme's own accent is the stock Orchid, so it only needs an overlay while the palette is actually coming from the wallpaper instead.
        if (Preferences.Default.Get("device_colors", true) && OperatingSystem.IsAndroidVersionAtLeast(31))
            Theme.ApplyStyle(Resource.Style.JournalApp_DeviceAccent, force: true);

        base.OnCreate(savedInstanceState);

        // The web layer dodges the keyboard itself by watching the visual viewport, so keep the window static and let CSS animate the dodge.
        // Pre-11 WebViews don't track the ime in the visual viewport, so fall back to the legacy resize there.
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            Window.SetSoftInputMode(global::Android.Views.SoftInput.AdjustNothing);
        else
            Window.SetSoftInputMode(global::Android.Views.SoftInput.AdjustResize);

        var backCallback = new OnBackPressedCallbackProxy(() =>
        {
            var service = IPlatformApplication.Current.Services.GetService<KeyEventService>();

            // Go back to the last app if no dialogs or pages were handled.
            if (!service.OnBackButtonPressed())
                MoveTaskToBack(false);
        });

        OnBackPressedDispatcher.AddCallback(this, backCallback);
    }
}

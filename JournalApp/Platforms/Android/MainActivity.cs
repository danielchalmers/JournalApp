using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using JournalApp.Platforms.Android;

namespace JournalApp;

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleInstance, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

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

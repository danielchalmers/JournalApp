using Android.App;
using Android.Content.PM;
using Android.Views;

namespace JournalApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool DispatchKeyEvent(KeyEvent e)
    {
        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Up)
            {
                var service = IPlatformApplication.Current.Services.GetService<PageService>();

                // Consume the event if any subscriptions were invoked.
                if (service.OnBackButtonPressed())
                    return true;
            }
        }

        return base.DispatchKeyEvent(e);
    }
}

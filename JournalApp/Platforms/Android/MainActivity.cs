using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;

namespace JournalApp;

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleInstance, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    [Intent.ActionSend, Intent.ActionView],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "application/*",
    DataPathPattern = "*.journalapp")]
public class MainActivity : MauiAppCompatActivity
{
    public override bool DispatchKeyEvent(KeyEvent e)
    {
        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Up)
            {
                var service = IPlatformApplication.Current.Services.GetService<KeyEventService>();

                // Consume the event if any subscriptions were invoked.
                if (service.OnBackButtonPressed())
                    return true;
            }
        }

        return base.DispatchKeyEvent(e);
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);

        string filePath = null;
        Stream inputStream = null;

        if (intent.Action == Intent.ActionSend)
        {
            var file = intent.ClipData.GetItemAt(0);
            filePath = file.Uri.LastPathSegment;
            inputStream = ContentResolver.OpenInputStream(file.Uri);

        }
        else if (intent.Action == Intent.ActionView)
        {
            var path = intent.Data.Path;
            filePath = Path.GetFileName(path);
            inputStream = ContentResolver.OpenInputStream(intent.Data);
        }

        if (inputStream == null)
            return;

        var memoryStream = new MemoryStream();
        inputStream.CopyTo(memoryStream);

        filePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(filePath, ".journalapp"));
        File.WriteAllBytes(filePath, memoryStream.ToArray());

        App.OnNewIntent(this, filePath);
    }
}

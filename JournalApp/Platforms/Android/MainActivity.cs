using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using JournalApp.Platforms.Android;

namespace JournalApp;

[Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleInstance, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    [Intent.ActionSend, Intent.ActionView],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "application/*",
    DataPathPatterns = [
        "/.*\\.journalapp",
        "/.*\\..*\\.journalapp",
        "/.*\\..*\\..*\\.journalapp",
        "/.*\\..*\\..*\\..*\\.journalapp"
])]
[IntentFilter(
    [Intent.ActionSend, Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataSchemes = ["content", "file"],
    DataMimeType = "application/octet-stream",
    DataPathPatterns = [
        "/.*\\.journalapp",
        "/.*\\..*\\.journalapp",
        "/.*\\..*\\..*\\.journalapp",
        "/.*\\..*\\..*\\..*\\.journalapp"
])]
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

        OnNewIntent(Intent);
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);

        if (App.ActivatedFilePath != null)
            return; // Already importing.

        string filePath;
        Android.Net.Uri streamUri;
        if (intent.Action == Intent.ActionSend)
        {
            var file = intent.ClipData.GetItemAt(0);
            filePath = file.Uri.LastPathSegment;
            streamUri = file.Uri;
        }
        else if (intent.Action == Intent.ActionView)
        {
            var path = intent.Data.Path;
            filePath = Path.GetFileName(path);
            streamUri = intent.Data;
        }
        else
        {
            return;
        }

        // Only process files with .journalapp extension
        if (!filePath.EndsWith(".journalapp", StringComparison.OrdinalIgnoreCase))
            return;

        var inputStream = ContentResolver.OpenInputStream(streamUri);
        var memoryStream = new MemoryStream();
        inputStream.CopyTo(memoryStream);

        App.ActivatedFilePath = Path.Combine(Path.GetTempPath(), filePath);
        File.WriteAllBytes(App.ActivatedFilePath, memoryStream.ToArray());

        App.OnNewIntent(this);
    }
}

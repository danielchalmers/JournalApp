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
// Added because the above filter doesn't work in Samsung My Files - https://stackoverflow.com/questions/50407193/open-custom-filetype-in-samsung-file-explorer.
[IntentFilter(
    [Intent.ActionSend, Intent.ActionView],
    Categories = [Intent.CategoryDefault],
    DataSchemes = ["content", "file"],
    DataMimeType = "*/*"
)]
// Accept text sharing for creating notes.
[IntentFilter(
    [Intent.ActionSend],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "text/plain"
)]
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

        if (App.ActivatedFilePath != null || App.SharedText != null)
            return; // Already importing or handling shared text.

        // Handle shared text for creating notes.
        if (intent.Action == Intent.ActionSend && intent.Type == "text/plain")
        {
            var sharedText = intent.GetStringExtra(Intent.ExtraText);
            if (!string.IsNullOrWhiteSpace(sharedText))
            {
                App.SharedText = sharedText;
                App.OnNewIntent(this);
                return;
            }
        }

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

        var inputStream = ContentResolver.OpenInputStream(streamUri);
        var memoryStream = new MemoryStream();
        inputStream.CopyTo(memoryStream);

        App.ActivatedFilePath = Path.Combine(Path.GetTempPath(), filePath);
        File.WriteAllBytes(App.ActivatedFilePath, memoryStream.ToArray());

        App.OnNewIntent(this);
    }
}

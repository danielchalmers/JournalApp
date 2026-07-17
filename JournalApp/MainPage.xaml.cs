using Microsoft.AspNetCore.Components.WebView;

namespace JournalApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // The native WebView paints white before Blazor's first frame, which flashes hard against a dark splash, so let the themed page background show through instead.
    private void OnBlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
    {
#if ANDROID
        e.WebView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif WINDOWS
        e.WebView.DefaultBackgroundColor = Microsoft.UI.Colors.Transparent;
#endif
    }
}

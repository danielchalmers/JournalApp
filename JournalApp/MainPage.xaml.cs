using Microsoft.AspNetCore.Components.WebView;

namespace JournalApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

#if ANDROID
        // Only WebView 144 and up forward the system bar insets to CSS, so an older one keeps the page letterboxed inside the bars instead of drawing under them blind.
        if (!WebViewReportsSafeArea())
            SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
#endif
    }

#if ANDROID
    private static bool WebViewReportsSafeArea()
    {
        var version = Android.Webkit.WebView.CurrentWebViewPackage?.VersionName;
        return version is not null && int.TryParse(version.Split('.')[0], out var major) && major >= 144;
    }
#endif

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

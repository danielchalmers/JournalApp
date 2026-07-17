using Microsoft.AspNetCore.Components.WebView;

namespace JournalApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnBlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
    {
#if ANDROID
        IPlatformApplication.Current.Services.GetRequiredService<SafeAreaService>().Attach(e.WebView);
#endif
    }
}

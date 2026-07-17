namespace JournalApp;

/// <summary>
/// Bridges Android window insets into CSS custom properties so the edge-to-edge web UI can pad around the system bars.
/// </summary>
public sealed class SafeAreaService(ILogger<SafeAreaService> logger)
{
#if ANDROID
    private Android.Webkit.WebView _webView;
    private float _density = 1;
    private AndroidX.Core.Graphics.Insets _bars;
    private int _imeHeight;

    /// <summary>
    /// Watches the web view for inset changes and pushes each one into the page.
    /// </summary>
    public void Attach(Android.Webkit.WebView webView)
    {
        _webView = webView;
        _density = webView.Resources.DisplayMetrics.Density;

        AndroidX.Core.View.ViewCompat.SetOnApplyWindowInsetsListener(webView, new InsetsListener(this));
        AndroidX.Core.View.ViewCompat.RequestApplyInsets(webView);
    }

    private void OnInsetsChanged(AndroidX.Core.Graphics.Insets bars, int imeHeight)
    {
        if (bars.Equals(_bars) && imeHeight == _imeHeight)
            return;

        _bars = bars;
        _imeHeight = imeHeight;
        logger.LogDebug($"Window insets changed: {bars}, ime {imeHeight}");
        Apply();
    }

    private void Apply()
    {
        if (_webView == null || _bars == null)
            return;

        _webView.Post(() =>
        {
            // Edge-to-edge disables the adjustResize soft input mode, so shrink the content frame ourselves to keep dialogs and focused inputs above the keyboard.
            var contentFrame = Platform.CurrentActivity?.FindViewById(Android.Resource.Id.Content);
            if (contentFrame != null && contentFrame.PaddingBottom != _imeHeight)
                contentFrame.SetPadding(0, 0, 0, _imeHeight);

            // The view already ends at the keyboard when it's open, so the gesture bar inset only applies while it's closed.
            var bottom = _imeHeight > 0 ? 0 : _bars.Bottom;

            // CSS pixels are density-independent, same as dp with the app's unscaled viewport.
            var js = "document.documentElement.style.setProperty('--safe-area-inset-top', '" + Dp(_bars.Top) + "px');" +
                     "document.documentElement.style.setProperty('--safe-area-inset-right', '" + Dp(_bars.Right) + "px');" +
                     "document.documentElement.style.setProperty('--safe-area-inset-bottom', '" + Dp(bottom) + "px');" +
                     "document.documentElement.style.setProperty('--safe-area-inset-left', '" + Dp(_bars.Left) + "px');";

            _webView.EvaluateJavascript(js, null);
        });
    }

    private int Dp(int px) => (int)Math.Round(px / _density);

    private sealed class InsetsListener(SafeAreaService owner) : Java.Lang.Object, AndroidX.Core.View.IOnApplyWindowInsetsListener
    {
        public AndroidX.Core.View.WindowInsetsCompat OnApplyWindowInsets(Android.Views.View v, AndroidX.Core.View.WindowInsetsCompat insets)
        {
            var types = AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars() | AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout();
            var ime = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.Ime());
            owner.OnInsetsChanged(insets.GetInsets(types), ime.Bottom);
            return insets;
        }
    }
#endif

    /// <summary>
    /// Re-applies the last known insets in case the first dispatch beat the page load.
    /// </summary>
    public void Reapply()
    {
        logger.LogDebug("Reapplying safe area insets");

#if ANDROID
        Apply();

        if (_webView != null)
            AndroidX.Core.View.ViewCompat.RequestApplyInsets(_webView);
#endif
    }
}

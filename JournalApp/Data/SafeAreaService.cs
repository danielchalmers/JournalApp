namespace JournalApp;

/// <summary>
/// Bridges Android window insets into CSS custom properties so the edge-to-edge web UI can pad around the system bars.
/// </summary>
public sealed class SafeAreaService(ILogger<SafeAreaService> logger)
{
#if ANDROID
    private Android.Webkit.WebView _webView;
    private float _density = 1;
    private AndroidX.Core.Graphics.Insets _insets;

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

    private void OnInsetsChanged(AndroidX.Core.Graphics.Insets insets)
    {
        if (insets.Equals(_insets))
            return;

        _insets = insets;
        logger.LogDebug($"Window insets changed: {insets}");
        Apply();
    }

    private void Apply()
    {
        if (_webView == null || _insets == null)
            return;

        // CSS pixels are density-independent, same as dp with the app's unscaled viewport.
        var js = "document.documentElement.style.setProperty('--safe-area-inset-top', '" + Dp(_insets.Top) + "px');" +
                 "document.documentElement.style.setProperty('--safe-area-inset-right', '" + Dp(_insets.Right) + "px');" +
                 "document.documentElement.style.setProperty('--safe-area-inset-bottom', '" + Dp(_insets.Bottom) + "px');" +
                 "document.documentElement.style.setProperty('--safe-area-inset-left', '" + Dp(_insets.Left) + "px');";

        _webView.Post(() => _webView.EvaluateJavascript(js, null));
    }

    private int Dp(int px) => (int)Math.Round(px / _density);

    private sealed class InsetsListener(SafeAreaService owner) : Java.Lang.Object, AndroidX.Core.View.IOnApplyWindowInsetsListener
    {
        public AndroidX.Core.View.WindowInsetsCompat OnApplyWindowInsets(Android.Views.View v, AndroidX.Core.View.WindowInsetsCompat insets)
        {
            var types = AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars() | AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout();
            owner.OnInsetsChanged(insets.GetInsets(types));
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

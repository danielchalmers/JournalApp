namespace JournalApp;

/// <summary>
/// Plays semantic haptic cues for key interactions.
/// On Android this goes through the decor view's PerformHapticFeedback, which needs no VIBRATE permission and respects the system touch feedback setting; elsewhere it's a no-op.
/// </summary>
public sealed class HapticFeedbackService
{
    /// <summary>
    /// A light tap for opening a picker or similar minor interaction.
    /// </summary>
    public void Click()
    {
#if ANDROID
        Perform(Android.Views.FeedbackConstants.VirtualKey);
#endif
    }

    /// <summary>
    /// A heavy buzz for destructive actions like a permanent delete.
    /// </summary>
    public void LongPress()
    {
#if ANDROID
        Perform(Android.Views.FeedbackConstants.LongPress);
#endif
    }

    /// <summary>
    /// Signals an action completed successfully, like submitting a dialog.
    /// </summary>
    public void Confirm()
    {
#if ANDROID
        // Confirm/Reject exist since API 30.
        Perform(OperatingSystem.IsAndroidVersionAtLeast(30) ? Android.Views.FeedbackConstants.Confirm : Android.Views.FeedbackConstants.KeyboardTap);
#endif
    }

    /// <summary>
    /// Signals an action was denied, like failing validation.
    /// </summary>
    public void Reject()
    {
#if ANDROID
        Perform(OperatingSystem.IsAndroidVersionAtLeast(30) ? Android.Views.FeedbackConstants.Reject : Android.Views.FeedbackConstants.LongPress);
#endif
    }

    /// <summary>
    /// A switch or binary choice turning on.
    /// </summary>
    public void ToggleOn()
    {
#if ANDROID
        // ToggleOn/ToggleOff and SegmentTick exist since API 34.
        Perform(OperatingSystem.IsAndroidVersionAtLeast(34) ? Android.Views.FeedbackConstants.ToggleOn : Android.Views.FeedbackConstants.ClockTick);
#endif
    }

    /// <summary>
    /// A switch or binary choice turning off.
    /// </summary>
    public void ToggleOff()
    {
#if ANDROID
        Perform(OperatingSystem.IsAndroidVersionAtLeast(34) ? Android.Views.FeedbackConstants.ToggleOff : Android.Views.FeedbackConstants.ClockTick);
#endif
    }

    /// <summary>
    /// A discrete step through segmented values, like picking a mood or nudging sleep hours.
    /// </summary>
    public void Tick()
    {
#if ANDROID
        Perform(OperatingSystem.IsAndroidVersionAtLeast(34) ? Android.Views.FeedbackConstants.SegmentTick : Android.Views.FeedbackConstants.ClockTick);
#endif
    }

#if ANDROID
    private static void Perform(Android.Views.FeedbackConstants constant) =>
        Platform.CurrentActivity?.Window?.DecorView?.PerformHapticFeedback(constant);
#endif
}

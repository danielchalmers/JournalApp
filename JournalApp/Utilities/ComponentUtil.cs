using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JournalApp;

internal static class ComponentUtil
{
    public static IDialogReference Show<TComponent>(this IDialogService dialogService, DialogOptions options) where TComponent : ComponentBase =>
        dialogService.Show<TComponent>(string.Empty, options);

    public static IDialogReference Show<TComponent>(this IDialogService dialogService, DialogParameters parameters) where TComponent : ComponentBase =>
        dialogService.Show<TComponent>(string.Empty, parameters);

    public static IDialogReference Show<TComponent>(this IDialogService dialogService, DialogParameters parameters, DialogOptions options) where TComponent : ComponentBase =>
        dialogService.Show<TComponent>(string.Empty, parameters, options);

    public static async Task<bool?> ShowCustomMessageBox(this IDialogService dialogService, string message, string yesText = "OK",
        string noText = null, string cancelText = null, DialogOptions options = null, bool showFeedbackLink = true)
    {
        var messageBoxOptions = new MessageBoxOptions
        {
            Title = string.Empty,
            Message = message,
            YesText = yesText,
            NoText = noText,
            CancelText = cancelText,
        };

        var parameters = new DialogParameters()
        {
            [nameof(MessageBoxOptions.Title)] = messageBoxOptions.Title,
            [nameof(MessageBoxOptions.Message)] = messageBoxOptions.Message,
            [nameof(MessageBoxOptions.MarkupMessage)] = messageBoxOptions.MarkupMessage,
            [nameof(MessageBoxOptions.CancelText)] = messageBoxOptions.CancelText,
            [nameof(MessageBoxOptions.NoText)] = messageBoxOptions.NoText,
            [nameof(MessageBoxOptions.YesText)] = messageBoxOptions.YesText,
            [nameof(CustomMessageBox.ShowFeedbackLink)] = showFeedbackLink,
        };

        var reference = await dialogService.ShowAsync<CustomMessageBox>(title: messageBoxOptions.Title, parameters: parameters, options: options);
        var result = await reference.Result;

        if (result.Canceled || result.Data is not bool data)
            return null;

        return data;
    }

    public static void ShowTeachingTip(this ISnackbar snackbar, string name, string text, bool oneTime = false)
    {
        if (Preferences.Get($"tip_{name}", string.Empty) == "seen")
            return;

        snackbar.Add(text, Severity.Info);

        if (oneTime)
            Preferences.Set($"tip_{name}", "seen");
    }

    public static void TeachingTipActionTaken(this ISnackbar snackbar, string name)
    {
        snackbar.Clear();

        Preferences.Set($"tip_{name}", "seen");
    }
}

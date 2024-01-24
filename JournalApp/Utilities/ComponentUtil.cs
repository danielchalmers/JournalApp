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

    public static async Task<bool?> ShowCustomMessageBox(this IDialogService dialogService, string title, string message, string yesText = "OK",
        string noText = null, string cancelText = null, DialogOptions options = null, bool showFeedbackLink = true)
    {
        var messageBoxOptions = new MessageBoxOptions
        {
            Title = title,
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

    public static void ShowOneTimeTip(this ISnackbar snackbar, string name, string text)
    {
        if (Preferences.Get($"help_{name}", false))
            return;

        Preferences.Set($"help_{name}", true);

        snackbar.Add(text, Severity.Normal, config => config.Icon = Icons.Material.Filled.Info);
    }
}

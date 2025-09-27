using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JournalApp;

internal static class ComponentUtil
{
    public static Task<IDialogReference> ShowAsync<TComponent>(this IDialogService dialogService, DialogOptions options) where TComponent : ComponentBase =>
        dialogService.ShowAsync<TComponent>(string.Empty, options);

    public static Task<IDialogReference> ShowAsync<TComponent>(this IDialogService dialogService, DialogParameters parameters) where TComponent : ComponentBase =>
        dialogService.ShowAsync<TComponent>(string.Empty, parameters);

    public static Task<IDialogReference> ShowAsync<TComponent>(this IDialogService dialogService, DialogParameters parameters, DialogOptions options) where TComponent : ComponentBase =>
        dialogService.ShowAsync<TComponent>(string.Empty, parameters, options);

    public static async Task<bool?> ShowJaMessageBox(this IDialogService dialogService, string message, string yesText = "OK",
        string noText = null, string cancelText = null, DialogOptions options = null, bool showFeedbackLink = false)
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
            [nameof(JaMessageBox.ShowFeedbackLink)] = showFeedbackLink,
        };

        var reference = await dialogService.ShowAsync<JaMessageBox>(title: messageBoxOptions.Title, parameters: parameters, options: options);
        var result = await reference.Result;

        if (result.Canceled || result.Data is not bool data)
            return null;

        return data;
    }
}

using Microsoft.AspNetCore.Components;

namespace JournalApp;

internal static class ComponentUtil
{
    public static IDialogReference Show<TComponent>(this IDialogService dialogService, DialogOptions options) where TComponent : ComponentBase =>
        dialogService.Show<TComponent>(string.Empty, options);

    public static IDialogReference Show<TComponent>(this IDialogService dialogService, DialogParameters parameters) where TComponent : ComponentBase =>
        dialogService.Show<TComponent>(string.Empty, parameters);

    public static IDialogReference Show<TComponent>(this IDialogService dialogService, DialogParameters parameters, DialogOptions options) where TComponent : ComponentBase =>
        dialogService.Show<TComponent>(string.Empty, parameters, options);
}

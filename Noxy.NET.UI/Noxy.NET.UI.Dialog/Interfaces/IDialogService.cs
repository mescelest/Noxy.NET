using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Interfaces;

public interface IDialogService
{
    event EventHandler<DialogChangedEventArgs>? DialogChanged;

    DialogContext Show(RenderFragment fragment);
    DialogContext Show(DialogOptions options, RenderFragment fragment);
    DialogContext Show<T>(Dictionary<string, object?> parameters) where T : ComponentBase;
    DialogContext Show<T>(DialogOptions options, Dictionary<string, object?> parameters) where T : ComponentBase;

    void Close(Guid id);
}

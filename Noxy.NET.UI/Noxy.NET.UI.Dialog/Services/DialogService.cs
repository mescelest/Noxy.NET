using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Services;

public class DialogService : IDialogService
{
    private List<Guid> Queue { get; set; } = [];
    private Dictionary<Guid, DialogContext> Collection { get; set; } = [];

    public DialogContext? Current => Collection.TryGetValue(Queue.FirstOrDefault(), out DialogContext? context) ? context : null;

    public event EventHandler<DialogChangedEventArgs>? DialogChanged;

    public DialogContext Show(RenderFragment fragment)
    {
        return AddDialogContext(new() { Fragment = fragment, Options = new() });
    }

    public DialogContext Show(DialogOptions options, RenderFragment fragment)
    {
        return AddDialogContext(new() { Fragment = fragment, Options = options });
    }

    public DialogContext Show<T>(Dictionary<string, object?> parameters) where T : ComponentBase
    {
        return AddDialogContext(new() { Fragment = GenerateDynamicFragment(typeof(T), parameters), Options = new() });
    }

    public DialogContext Show<T>(DialogOptions options, Dictionary<string, object?> parameters) where T : ComponentBase
    {
        return AddDialogContext(new() { Fragment = GenerateDynamicFragment(typeof(T), parameters), Options = options });
    }

    public void Close(Guid id)
    {
        int index = Queue.IndexOf(id);
        if (index == -1) return;

        Queue.RemoveAt(index);
        Collection.Remove(id);
        if (index == 0)
        {
            DialogChanged?.Invoke(this, new(Current));
        }
    }

    private DialogContext AddDialogContext(DialogContext context)
    {
        Queue.Add(context.ID);
        Collection[context.ID] = context;
        if (Queue.Count == 1)
        {
            DialogChanged?.Invoke(this, new(context));
        }

        return context;
    }

    private static RenderFragment GenerateDynamicFragment(Type type, Dictionary<string, object?> parameters)
    {
        return builder =>
        {
            builder.OpenComponent<DynamicComponent>(1);
            builder.AddAttribute(2, "Type", type);
            builder.AddAttribute(3, "Parameters", parameters);
            builder.CloseComponent();
        };
    }
}

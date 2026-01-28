using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseComponentForm<TForm> : ElementComponent where TForm : class
{
    [Parameter, EditorRequired]
    public EventCallback<WebFormContext<TForm>> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected WebFormContext<TForm> Context { get; set; } = null!;

    protected abstract WebFormContext<TForm> CreateContext();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Context = CreateContext();
    }
}

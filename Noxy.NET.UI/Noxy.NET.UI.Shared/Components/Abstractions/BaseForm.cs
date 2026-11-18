using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Models.Forms;

namespace Noxy.NET.UI.Components.Abstractions;

public abstract class BaseForm<TModel> : BaseComponentElement where TModel : class
{
    [Parameter, EditorRequired]
    public EventCallback<WebFormContext<TModel>> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public WebFormContext<TModel> Context { get; private set; } = null!;
    public TModel Model { get; private set; } = null!;

    protected abstract WebFormContext<TModel> CreateContext();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Context = CreateContext();
        Model = Context.Model;
    }

    protected Task ForwardSubmit(WebFormContext<TModel> ctx)
    {
        return OnSubmit.InvokeAsync(ctx);
    }
}

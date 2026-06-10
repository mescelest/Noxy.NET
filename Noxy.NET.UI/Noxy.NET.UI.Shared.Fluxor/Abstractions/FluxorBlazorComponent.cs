using Fluxor.Blazor.Web.Components;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class FluxorBlazorComponent : FluxorComponent, IBlazorComponent
{
    public ComponentMetadata Metadata { get; }

    public Guid UUID => Metadata.UUID;
    public string UUIDString => Metadata.UUIDString;
    public string UUIDCode => Metadata.UUIDCode;
    
    public bool IsRendered => Metadata.IsRendered;

    public string ComponentName => Metadata.Name;

    public virtual string CssClass => ComponentName;

    protected FluxorBlazorComponent()
    {
        Metadata = new(GetType());
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (!firstRender) return;

        Metadata.MarkAsRendered();
        OnAfterFirstRender();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        await OnAfterFirstRenderAsync();
    }

    protected virtual void OnAfterFirstRender()
    {
    }

    protected virtual Task OnAfterFirstRenderAsync()
    {
        return Task.CompletedTask;
    }
}

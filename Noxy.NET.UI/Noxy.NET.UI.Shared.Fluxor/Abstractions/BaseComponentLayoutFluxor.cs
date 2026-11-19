using Fluxor.Blazor.Web.Components;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseComponentLayoutFluxor : FluxorLayout, ILayoutComponent
{
    public ComponentMetadata Metadata { get; }

    public string CssClass => Metadata.Name;

    protected BaseComponentLayoutFluxor()
    {
        Metadata = new(GetType());
    }
}

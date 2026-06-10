using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class LayoutComponent : LayoutComponentBase, ILayoutComponent
{
    public ComponentMetadata Metadata { get; }

    public string CssClass => Metadata.Name;

    protected LayoutComponent()
    {
        Metadata = new(GetType());
    }
}

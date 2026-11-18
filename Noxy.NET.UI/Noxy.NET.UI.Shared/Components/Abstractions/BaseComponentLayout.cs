using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Components.Abstractions;

public abstract class BaseComponentLayout : LayoutComponentBase, ILayoutComponent
{
    public ComponentMetadata Metadata { get; }

    public string CssClass => Metadata.Name;

    protected BaseComponentLayout()
    {
        Metadata = new(GetType());
    }
}

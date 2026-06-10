using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Interfaces;

public interface ILayoutComponent
{
    ComponentMetadata Metadata { get; }

    string CssClass { get; }
}

using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Interfaces;

public interface IBlazorComponent
{
    ComponentMetadata Metadata { get; }

    Guid UUID { get; }
    string UUIDString { get; }
    string UUIDCode { get; }
    string CssClass { get; }
    bool IsRendered { get; }
}

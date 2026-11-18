using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Components.Abstractions;

public abstract class BaseComponentPage : BaseComponentBlazor, IPageComponent
{
    public override string CssClass => ComponentMetadata.CombineCssClass(base.CssClass, "Page");
}

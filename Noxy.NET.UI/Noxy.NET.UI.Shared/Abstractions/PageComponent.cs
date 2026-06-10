using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract class PageComponent : BlazorComponent, IPageComponent
{
    public override string CssClass => ComponentMetadata.CombineCssClass(base.CssClass, "Page");
}

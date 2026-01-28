namespace Noxy.NET.UI.Abstractions;

public abstract class PageComponent : BlazorComponent
{
    protected override string CssClass => CombineCssClass(base.CssClass, "Page");
}

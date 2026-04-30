namespace Noxy.NET.EntityManagement.Presentation.Abstractions;

public abstract class FluxorPageComponent : FluxorElementComponent
{
    protected override string CssClass => CombineCssClass(base.CssClass, "Page");
}

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class FluxorPageComponent : FluxorElementComponent
{
    protected override string CssClass => CombineCssClass(base.CssClass, "Page");
}

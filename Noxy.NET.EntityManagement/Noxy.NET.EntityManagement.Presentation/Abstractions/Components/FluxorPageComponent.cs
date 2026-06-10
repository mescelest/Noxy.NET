namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class FluxorPageComponent : FluxorElementComponent
{
    public override string CssClass => ComponentMetadata.CombineCssClass(base.CssClass, "Page");
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Noxy.NET.UI.Components.Abstractions;

namespace Noxy.NET.UI.Components.Elements;

public class DynamicHTMLTag : BaseComponentElement
{
    [Parameter]
    public string? Tag { get; set; }
    private string TagCurrent => !string.IsNullOrWhiteSpace(Tag) ? Tag : "div";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.OpenElement(0, TagCurrent);

        if (AdditionalAttributes?.Count > 0)
        {
            builder.AddMultipleAttributes(1, AdditionalAttributes);
        }

        if (ChildContent != null)
        {
            builder.AddContent(2, ChildContent);
        }

        builder.CloseElement();
    }
}

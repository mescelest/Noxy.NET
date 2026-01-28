using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Components;

public class DynamicHTMLTag : ElementComponent
{
    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(Tag);

        builder.OpenElement(0, Tag);

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

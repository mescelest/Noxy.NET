using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseGraphViewer : ElementComponent, IGraphViewer
{
    public IGraphContext Context { get; set; } = null!;
    public ElementReference ReferenceToSVG { get; set; }

    [Parameter]
    public EventCallback<IGraphDragContext> OnDragCommit { get; set; }
}

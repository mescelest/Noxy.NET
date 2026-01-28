using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Abstractions;

public abstract class GraphViewerTool : ElementComponent
{
    [CascadingParameter]
    internal IGraphViewer Parent { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GraphViewerTool a = this;
    }
}

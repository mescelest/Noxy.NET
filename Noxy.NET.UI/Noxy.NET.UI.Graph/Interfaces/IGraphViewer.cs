using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Interfaces;

public interface IGraphViewer
{
    IGraphContext Context { get; set; }
    ElementReference ReferenceToSVG { get; set; }

    EventCallback<IGraphDragContext> OnDragCommit { get; set; }
}

public interface IGraphViewer<T> : IGraphViewer
{
    new IGraphContext Context { get; set; }
}

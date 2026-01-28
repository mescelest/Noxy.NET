using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Interfaces;

public interface IGraphContext
{
    int Width { get; set; }
    int Height { get; set; }
    int X { get; }
    int Y { get; }

    GraphDragContext? Drag { get; set; }
    GraphZoomContext? Zoom { get; set; }

    List<IGraphNode> Data { get; set; }
}

public interface IGraphContext<T> : IGraphContext
{
    new List<IGraphNode<T>> Data { get; set; }
}

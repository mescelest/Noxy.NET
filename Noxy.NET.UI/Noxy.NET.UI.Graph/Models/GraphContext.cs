using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class GraphContext : IGraphContext
{
    public int Width { get; set; } = 2000;
    public int Height { get; set; } = 2000;
    public int X => -Width / 2;
    public int Y => -Height / 2;

    public List<IGraphNode> Data { get; set; } = [];

    public GraphDragContext? Drag { get; set; } = new();
    public GraphZoomContext? Zoom { get; set; } = new();
}

public class GraphContext<T>(IEnumerable<IGraphNode<T>> data) : GraphContext, IGraphContext<T>
{
    public new List<IGraphNode<T>> Data { get; set; } = data.ToList();
}

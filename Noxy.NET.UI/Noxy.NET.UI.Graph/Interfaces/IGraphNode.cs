namespace Noxy.NET.UI.Interfaces;

public interface IGraphNode
{
    double X { get; set; }
    double Y { get; set; }
    object? Data { get; set; }
    IGraphNode? Parent { get; set; }
    List<IGraphNode> Children { get; set; }
}

public interface IGraphNode<TData> : IGraphNode
{
    new TData Data { get; set; }
    new IGraphNode<TData>? Parent { get; set; }
    new List<IGraphNode<TData>> Children { get; set; }
}

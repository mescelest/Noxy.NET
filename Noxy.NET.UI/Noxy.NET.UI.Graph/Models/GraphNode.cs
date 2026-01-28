using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class GraphNode(double x = 0, double y = 0) : IGraphNode
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public object? Data { get; set; }
    public IGraphNode? Parent { get; set; }
    public List<IGraphNode> Children { get; set; } = [];
}

public class GraphNode<T> : GraphNode, IGraphNode<T>
{
    public GraphNode(T data) : this(0, 0, data, [])
    {
    }

    public GraphNode(T data, IEnumerable<IGraphNode<T>> children) : this(0, 0, data, children)
    {
    }

    public GraphNode(double x, double y, T data) : this(x, y, data, [])
    {
    }

    public GraphNode(double x, double y, T data, IEnumerable<IGraphNode<T>> children) : base(x, y)
    {
        Data = data;
        foreach (IGraphNode<T> child in children)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }

    public new T Data { get; set; }
    public new IGraphNode<T>? Parent { get; set; }
    public new List<IGraphNode<T>> Children { get; set; } = [];
}

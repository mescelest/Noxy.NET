using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class GraphPath(IGraphNode parent, IGraphNode child) : IGraphPath
{
    public IGraphNode Parent { get; set; } = parent;
    public IGraphNode Child { get; set; } = child;

    public double X1 => Parent.X;
    public double Y1 => Parent.Y;

    public double X2 => Child.X;
    public double Y2 => Child.Y;
}

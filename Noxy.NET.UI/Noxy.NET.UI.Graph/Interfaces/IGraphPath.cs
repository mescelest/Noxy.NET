namespace Noxy.NET.UI.Interfaces;

public interface IGraphPath
{
    double X1 { get; }
    double X2 { get; }
    double Y1 { get; }
    double Y2 { get; }
    IGraphNode Parent { get; set; }
    IGraphNode Child { get; set; }
}

public interface IGraphPath<T> : IGraphPath
{
    new IGraphNode<T> Parent { get; set; }
    new IGraphNode<T> Child { get; set; }
}

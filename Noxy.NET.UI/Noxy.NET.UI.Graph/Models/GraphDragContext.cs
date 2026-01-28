using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class GraphDragContext : IGraphDragContext
{
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Speed { get; set; } = 1.5;

    public void UpdatePosition()
    {
    }
}

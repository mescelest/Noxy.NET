using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class GraphZoomContext : IGraphZoomContext
{
    public double Level { get; set; } = 1.0;
    public double Speed { get; set; } = 0.2;
    public double Min { get; set; } = 0.2;
    public double Max { get; set; } = 3;
}

namespace Noxy.NET.UI.Interfaces;

public interface IGraphZoomContext
{
    double Level { get; set; }
    double Speed { get; set; }
    double Min { get; set; }
    double Max { get; set; }
}

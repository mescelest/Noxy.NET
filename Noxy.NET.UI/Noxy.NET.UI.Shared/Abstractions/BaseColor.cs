using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract record BaseColor
{
    protected const float Tolerance = 0.00001f;

    public abstract string ToCssString();

    public abstract RgbColor ToRgb();
    public abstract HexColor ToHex();
    public abstract HslColor ToHsl();
    public abstract HsvColor ToHsv();
    public abstract XyzColor ToXyz();
    public abstract LabColor ToLab();
    public abstract LchColor ToLch();
    public abstract OkLabColor ToOkLab();
    public abstract OkLchColor ToOkLch();
}

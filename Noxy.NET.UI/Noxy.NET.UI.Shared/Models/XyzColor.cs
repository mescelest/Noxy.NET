using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record XyzColor(double X, double Y, double Z, float Alpha = 1.0f) : BaseColor
{
    public override string ToCssString() => throw new NotSupportedException("XYZ coordinates are not directly supported in native CSS specifications.");

    public override RgbColor ToRgb()
    {
        double r = X * 3.2404542 + Y * -1.5371385 + Z * -0.4985314;
        double g = X * -0.9692660 + Y * 1.8760108 + Z * 0.0415560;
        double b = X * 0.0556434 + Y * -0.2040259 + Z * 1.0572252;

        return new(Compand(r), Compand(g), Compand(b));
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => this;
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => throw new NotImplementedException();
    public override OkLabColor ToOkLab() => throw new NotImplementedException();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    private static int Compand(double v)
    {
        v = v > 0.0031308 ? 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055 : 12.92 * v;
        return (int)Math.Clamp(Math.Round(v * 255), 0, 255);
    }
}

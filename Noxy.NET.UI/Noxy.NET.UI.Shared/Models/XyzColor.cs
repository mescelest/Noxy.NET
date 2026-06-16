using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record XyzColor : BaseColor
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public override double Alpha { get; }

    public XyzColor(double x, double y, double z, double alpha = 1.0)
    {
        X = x;
        Y = y;
        Z = z;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => throw new NotSupportedException("XYZ coordinates are not directly supported in CSS.");

    public override RgbColor ToRgb()
    {
        double r = X * 3.2404542 + Y * -1.5371385 + Z * -0.4985314;
        double g = X * -0.9692660 + Y * 1.8760108 + Z * 0.0415560;
        double b = X * 0.0556434 + Y * -0.2040259 + Z * 1.0572252;

        return new(ApplyGamma(r), ApplyGamma(g), ApplyGamma(b), Alpha);
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => this;
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    private static int ApplyGamma(double v)
    {
        v = v > 0.0031308
            ? 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055
            : 12.92 * v;

        return (int)Math.Clamp(Math.Round(v * 255), 0, 255);
    }

    public void Deconstruct(out double X, out double Y, out double Z, out double Alpha)
    {
        X = this.X;
        Y = this.Y;
        Z = this.Z;
        Alpha = this.Alpha;
    }
}

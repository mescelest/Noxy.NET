using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record LchColor : BaseColor
{
    public double L { get; }
    public double C { get; }
    public double H { get; }
    public double Alpha { get; }

    public LchColor(double l, double c, double h, double alpha = 1.0)
    {
        L = Math.Clamp(l, 0.0, 100.0);
        C = Math.Max(c, 0.0);
        H = ((h % 360) + 360) % 360;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"lch({L}% {C} {H})" : $"lch({L}% {C} {H} / {Alpha})";

    public override LabColor ToLab()
    {
        double hRad = H * (Math.PI / 180.0);
        double a = C * Math.Cos(hRad);
        double b = C * Math.Sin(hRad);

        return new(L, a, b, Alpha);
    }

    public override RgbColor ToRgb() => ToLab().ToRgb();
    public override HexColor ToHex() => ToLab().ToHex();
    public override HslColor ToHsl() => ToLab().ToHsl();
    public override HsvColor ToHsv() => ToLab().ToHsv();
    public override XyzColor ToXyz() => ToLab().ToXyz();
    public override LchColor ToLch() => this;
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToOkLab().ToOkLch();
}

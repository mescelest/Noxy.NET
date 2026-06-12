using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record OkLchColor : BaseColor
{
    public double L { get; }
    public double C { get; }
    public double H { get; }
    public double Alpha { get; }

    public OkLchColor(double l, double c, double h, double alpha = 1.0)
    {
        L = Math.Clamp(l, 0.0, 1.0);
        C = Math.Max(c, 0.0);
        H = ((h % 360) + 360) % 360;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"oklch({L * 100}% {C} {H})" : $"oklch({L * 100}% {C} {H} / {Alpha})";

    public override RgbColor ToRgb()
    {
        double hRad = H * Math.PI / 180.0;
        double a = C * Math.Cos(hRad);
        double b = C * Math.Sin(hRad);

        double lPrime = L + 0.3963377774 * a + 0.2158037573 * b;
        double mPrime = L - 0.1055613458 * a - 0.0638541728 * b;
        double sPrime = L - 0.0894841775 * a - 1.2914855480 * b;

        double l = lPrime * lPrime * lPrime;
        double m = mPrime * mPrime * mPrime;
        double s = sPrime * sPrime * sPrime;

        double rLinear = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        double gLinear = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        double bLinear = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        return new(
            ApplyGamma(rLinear),
            ApplyGamma(gLinear),
            ApplyGamma(bLinear),
            Alpha
        );
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLchColor ToOkLch() => this;

    public override OkLabColor ToOkLab()
    {
        double hRad = H * Math.PI / 180.0;
        double a = C * Math.Cos(hRad);
        double b = C * Math.Sin(hRad);
        return new OkLabColor(L, a, b, Alpha);
    }


    private static int ApplyGamma(double v)
    {
        v = v > 0.0031308
            ? 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055
            : 12.92 * v;

        return (int)Math.Clamp(Math.Round(v * 255), 0, 255);
    }
}

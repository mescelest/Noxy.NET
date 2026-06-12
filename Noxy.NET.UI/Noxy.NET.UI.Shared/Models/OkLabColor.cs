using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record OkLabColor : BaseColor
{
    public double L { get; }
    public double A { get; }
    public double B { get; }
    public double Alpha { get; }

    public OkLabColor(double l, double a, double b, double alpha = 1.0)
    {
        L = Math.Clamp(l, 0.0, 1.0);
        A = a;
        B = b;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"oklab({L} {A} {B})" : $"oklab({L} {A} {B} / {Alpha})";

    public override OkLabColor ToOkLab() => this;

    public override OkLchColor ToOkLch()
    {
        double c = Math.Sqrt(A * A + B * B);
        double h = Math.Atan2(B, A) * (180.0 / Math.PI);
        if (h < 0) h += 360.0;

        return new OkLchColor(L, c, h, Alpha);
    }

    public override XyzColor ToXyz()
    {
        double lPrime = L + 0.3963377774 * A + 0.2158037573 * B;
        double mPrime = L - 0.1055613458 * A - 0.0638541728 * B;
        double sPrime = L - 0.0894841775 * A - 1.2914855480 * B;

        double l = lPrime * lPrime * lPrime;
        double m = mPrime * mPrime * mPrime;
        double s = sPrime * sPrime * sPrime;

        double x = 1.2270138511 * l - 0.5577999807 * m + 0.2812561496 * s;
        double y = -0.0405801784 * l + 1.1122568696 * m - 0.0716766787 * s;
        double z = -0.0763812845 * l - 0.4214819784 * m + 1.5861632204 * s;

        return new XyzColor(x, y, z, Alpha);
    }

    public override LabColor ToLab() => ToXyz().ToLab();
    public override RgbColor ToRgb() => ToXyz().ToRgb();
    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override LchColor ToLch() => ToLab().ToLch();
}

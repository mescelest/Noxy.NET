using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record OkLabColor : BaseColor
{
    public double Lightness { get; }
    public double AxisA { get; }
    public double AxisB { get; }
    public override double Alpha { get; }

    public OkLabColor(double lightness, double axisA, double axisB, double alpha = 1.0)
    {
        Lightness = Math.Clamp(lightness, 0.0, 1.0);
        AxisA = axisA;
        AxisB = axisB;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"oklab({Lightness} {AxisA} {AxisB})" : $"oklab({Lightness} {AxisA} {AxisB} / {AlphaCssString})";

    public override OkLabColor ToOkLab() => this;

    public override OkLchColor ToOkLch()
    {
        double c = Math.Sqrt(AxisA * AxisA + AxisB * AxisB);
        double h = Math.Atan2(AxisB, AxisA) * (180.0 / Math.PI);
        if (h < 0) h += 360.0;

        return new(Lightness, c, h, Alpha);
    }

    public override XyzColor ToXyz()
    {
        double lPrime = Lightness + 0.3963377774 * AxisA + 0.2158037573 * AxisB;
        double mPrime = Lightness - 0.1055613458 * AxisA - 0.0638541728 * AxisB;
        double sPrime = Lightness - 0.0894841775 * AxisA - 1.2914855480 * AxisB;

        double l = lPrime * lPrime * lPrime;
        double m = mPrime * mPrime * mPrime;
        double s = sPrime * sPrime * sPrime;

        double x = 1.2270138511 * l - 0.5577999807 * m + 0.2812561496 * s;
        double y = -0.0405801784 * l + 1.1122568696 * m - 0.0716766787 * s;
        double z = -0.0763812845 * l - 0.4214819784 * m + 1.5861632204 * s;

        return new(x, y, z, Alpha);
    }

    public override LabColor ToLab() => ToXyz().ToLab();
    public override RgbColor ToRgb() => ToXyz().ToRgb();
    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override LchColor ToLch() => ToLab().ToLch();

    public static bool TryParse(string input, [NotNullWhen(true)] out OkLabColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("oklab", StringComparison.OrdinalIgnoreCase) || !TryPrepareFormat(span, 5, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenLightness, out ReadOnlySpan<char> rem1)) return false;
        if (!TryParsePercentageOrRaw(tokenLightness, out double lightness, percentageScale: 100.0)) return false;

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenAxisA, out ReadOnlySpan<char> rem2)) return false;
        if (!double.TryParse(tokenAxisA, CultureInfo.InvariantCulture, out double axisA)) return false;

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenAxisB, out ReadOnlySpan<char> rem3)) return false;
        if (!double.TryParse(tokenAxisB, CultureInfo.InvariantCulture, out double axisB)) return false;

        if (!TryReadAndParseAlpha(rem3, out double alpha)) return false;

        color = new(lightness, axisA, axisB, alpha);
        return true;
    }
}

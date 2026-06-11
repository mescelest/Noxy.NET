using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record RgbColor(int R, int G, int B, double Alpha = 1.0) : BaseColor
{
    public override string ToCssString() => $"rgba({R}, {G}, {B}, {Alpha})";

    public override RgbColor ToRgb() => this;

    public override HexColor ToHex()
    {
        int alphaByte = (int)Math.Round(Alpha * 255);

        return alphaByte == 255
            ? new($"#{R:X2}{G:X2}{B:X2}")
            : new($"#{R:X2}{G:X2}{B:X2}{alphaByte:X2}");
    }

    public override HslColor ToHsl()
    {
        double rf = R / 255.0, gf = G / 255.0, bf = B / 255.0;
        double max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf));
        double h = 0, s = 0, l = (max + min) / 2.0;

        if (Math.Abs(max - min) < Tolerance)
        {
            return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(l * 100), Alpha);
        }

        double d = max - min;
        s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        if (Math.Abs(max - rf) < Tolerance)
        {
            h = (gf - bf) / d + (gf < bf ? 6 : 0);
        }
        else if (Math.Abs(max - gf) < Tolerance)
        {
            h = (bf - rf) / d + 2;
        }
        else if (Math.Abs(max - bf) < Tolerance)
        {
            h = (rf - gf) / d + 4;
        }

        h /= 6.0;
        return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(l * 100), Alpha);
    }

    public override HsvColor ToHsv()
    {
        double rf = R / 255.0, gf = G / 255.0, bf = B / 255.0;
        double max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf));
        double h = 0, d = max - min;
        double s = max == 0 ? 0 : d / max;

        if (Math.Abs(max - min) < Tolerance)
        {
            return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(max * 100), Alpha);
        }

        if (Math.Abs(max - rf) < Tolerance)
        {
            h = (gf - bf) / d + (gf < bf ? 6 : 0);
        }
        else if (Math.Abs(max - gf) < Tolerance)
        {
            h = (bf - rf) / d + 2;
        }
        else if (Math.Abs(max - bf) < Tolerance)
        {
            h = (rf - gf) / d + 4;
        }

        h /= 6.0;
        return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(max * 100), Alpha);
    }

    public override XyzColor ToXyz()
    {
        double r = InverseSrgbCompand(R);
        double g = InverseSrgbCompand(G);
        double b = InverseSrgbCompand(B);

        double x = r * 0.4124564 + g * 0.3575761 + b * 0.1804375;
        double y = r * 0.2126729 + g * 0.7151522 + b * 0.0721750;
        double z = r * 0.0193339 + g * 0.1191920 + b * 0.9503041;

        return new(x, y, z);
    }

    public override LabColor ToLab()
    {
        (double x, double y, double z, float alpha) = ToXyz();

        double xr = x / 0.95047;
        double yr = y / 1.00000;
        double zr = z / 1.08883;

        double fx = ApplyPerceptualCurve(xr);
        double fy = ApplyPerceptualCurve(yr);
        double fz = ApplyPerceptualCurve(zr);

        double l = 116.0 * fy - 16.0;
        double a = 500.0 * (fx - fy);
        double b = 200.0 * (fy - fz);

        return new(l, a, b, alpha);
    }

    public override LchColor ToLch()
    {
        return ToXyz().ToLab().ToLch();
    }

    public override OkLabColor ToOkLab()
    {
        return ToXyz().ToOkLab();
    }

    public override OkLchColor ToOkLch()
    {
        double r = InverseSrgbCompand(R);
        double g = InverseSrgbCompand(G);
        double b = InverseSrgbCompand(B);

        double l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
        double m = 0.2119034982 * r + 0.6806995451 * g + 0.1073970004 * b;
        double s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

        double lCubeRoot = Math.Cbrt(l);
        double mCubeRoot = Math.Cbrt(m);
        double sCubeRoot = Math.Cbrt(s);

        double lOklab = 0.2104542553 * lCubeRoot + 0.7936177850 * mCubeRoot - 0.0040720403 * sCubeRoot;
        double aOklab = 1.9779984951 * lCubeRoot - 2.4285922050 * mCubeRoot + 0.4505937099 * sCubeRoot;
        double bOklab = 0.0259040371 * lCubeRoot + 0.7827717662 * mCubeRoot - 0.8086757660 * sCubeRoot;

        double chroma = Math.Sqrt(aOklab * aOklab + bOklab * bOklab);
        double hueAngle = Math.Atan2(bOklab, aOklab) * 180.0 / Math.PI;

        if (hueAngle < 0) hueAngle += 360.0;

        return new(lOklab, chroma, hueAngle, Alpha);
    }

    private static double InverseSrgbCompand(int byteValue)
    {
        double v = byteValue / 255.0;
        return v > 0.04045 ? Math.Pow((v + 0.055) / 1.055, 2.4) : v / 12.92;
    }

    private static double ApplyPerceptualCurve(double intensity)
    {
        const double delta = 0.20689655172413793;
        const double threshold = delta * delta * delta;

        return intensity > threshold
            ? Math.Cbrt(intensity)
            : intensity / (3.0 * delta * delta) + 4.0 / 29.0;
    }
}

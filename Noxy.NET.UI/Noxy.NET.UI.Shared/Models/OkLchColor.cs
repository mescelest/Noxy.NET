using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record OkLchColor(double L, double C, double H, double Alpha = 1.0) : BaseColor
{
    public override string ToCssString() => $"oklch({L * 100}% {C} {H} / {Alpha})";

    public override RgbColor ToRgb()
    {
        double hueRadians = H * Math.PI / 180.0;
        double aOkLab = C * Math.Cos(hueRadians);
        double bOkLab = C * Math.Sin(hueRadians);

        double l = L + 0.3963377774 * aOkLab + 0.2158037573 * bOkLab;
        double m = L - 0.1055613458 * aOkLab - 0.0638541728 * bOkLab;
        double s = L - 0.0894841775 * aOkLab - 1.2914855480 * bOkLab;

        l = Math.Pow(l, 3);
        m = Math.Pow(m, 3);
        s = Math.Pow(s, 3);

        double rLinear = +4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        double gLinear = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        double bLinear = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        return new(Compand(rLinear), Compand(gLinear), Compand(bLinear), Alpha);
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLchColor ToOkLch() => this;
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();

    private static int Compand(double v)
    {
        v = v > 0.0031308 ? 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055 : 12.92 * v;
        return (int)Math.Clamp(Math.Round(v * 255), 0, 255);
    }
}

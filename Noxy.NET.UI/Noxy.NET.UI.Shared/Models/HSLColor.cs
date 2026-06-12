using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HslColor : BaseColor
{
    public int H { get; }
    public int S { get; }
    public int L { get; }
    public double Alpha { get; }

    public HslColor(int h, int s, int l, double alpha = 1.0)
    {
        H = ((h % 360) + 360) % 360;
        S = Math.Clamp(s, 0, 100);
        L = Math.Clamp(l, 0, 100);
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"hsl({H} {S}% {L}%)" : $"hsla({H} {S}% {L}% / {Alpha})";

    public override RgbColor ToRgb()
    {
        double h = H / 360.0;
        double s = S / 100.0;
        double l = L / 100.0;

        if (s == 0)
        {
            int gray = (int)Math.Round(l * 255);
            return new(gray, gray, gray, Alpha);
        }

        double q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
        double p = 2.0 * l - q;

        int r = (int)Math.Round(HueToRgb(p, q, h + 1.0 / 3.0) * 255);
        int g = (int)Math.Round(HueToRgb(p, q, h) * 255);
        int b = (int)Math.Round(HueToRgb(p, q, h - 1.0 / 3.0) * 255);

        return new(r, g, b, Alpha);

        static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1.0;
            if (t > 1) t -= 1.0;

            return t switch
            {
                < 1.0 / 6.0 => p + (q - p) * 6.0 * t,
                < 1.0 / 2.0 => q,
                < 2.0 / 3.0 => p + (q - p) * (2.0 / 3.0 - t) * 6.0,
                _ => p
            };
        }
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => this;
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();
}

using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HsvColor : BaseColor
{
    public int H { get; }
    public int S { get; }
    public int V { get; }
    public double Alpha { get; }

    public HsvColor(int h, int s, int v, double alpha = 1.0)
    {
        H = ((h % 360) + 360) % 360;
        S = Math.Clamp(s, 0, 100);
        V = Math.Clamp(v, 0, 100);
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"hsl({H} {S}% {V}%)" : $"hsla({H} {S}% {V}% / {Alpha})";

    public override RgbColor ToRgb()
    {
        double h = H / 360.0;
        double s = S / 100.0;
        double v = V / 100.0;
        double a = Alpha;

        if (s == 0)
        {
            int gray = (int)Math.Round(v * 255);
            return new(gray, gray, gray, a);
        }

        double h6 = h * 6.0;
        int sector = (int)Math.Floor(h6);
        double fraction = h6 - sector;

        double p = v * (1.0 - s);
        double q = v * (1.0 - s * fraction);
        double t = v * (1.0 - s * (1.0 - fraction));

        (double rf, double gf, double bf) = sector switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q)
        };

        return new(
            (int)Math.Round(rf * 255),
            (int)Math.Round(gf * 255),
            (int)Math.Round(bf * 255),
            a
        );
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => this;
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    public static bool TryParse(string input, out HexColor? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();
        if (input[0] != '#' || input.Length is not 4 and not 5 and not 7 and not 9) return false;

        for (int i = 1; i < input.Length; i++)
        {
            if (!Uri.IsHexDigit(input[i])) return false;
        }

        color = new HexColor(input);
        return true;
    }
}

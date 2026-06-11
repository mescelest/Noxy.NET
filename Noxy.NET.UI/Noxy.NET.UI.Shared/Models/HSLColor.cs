using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HslColor(int H, int S, int L, double Alpha = 1.0) : BaseColor
{
    public override string ToCssString() => $"hsla({H}, {S}%, {L}%, {Alpha})";

    public override RgbColor ToRgb()
    {
        double hf = H / 360.0, sf = S / 100.0, lf = L / 100.0;
        if (S == 0)
        {
            int gray = (int)Math.Round(lf * 255);
            return new(gray, gray, gray, Alpha);
        }

        double q = lf < 0.5 ? lf * (1.0 + sf) : lf + sf - lf * sf;
        double p = 2.0 * lf - q;

        int r = (int)Math.Round(HueToRgb(p, q, hf + 1.0 / 3.0) * 255);
        int g = (int)Math.Round(HueToRgb(p, q, hf) * 255);
        int b = (int)Math.Round(HueToRgb(p, q, hf - 1.0 / 3.0) * 255);

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
    public override LchColor ToLch() => throw new NotImplementedException();
    public override OkLabColor ToOkLab() => throw new NotImplementedException();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();
}

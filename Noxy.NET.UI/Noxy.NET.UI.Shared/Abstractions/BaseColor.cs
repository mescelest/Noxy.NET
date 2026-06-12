using System.Diagnostics.CodeAnalysis;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract record BaseColor
{
    protected const float Tolerance = 0.00001f;

    public abstract string ToCssString();

    public abstract RgbColor ToRgb();
    public abstract HexColor ToHex();
    public abstract HslColor ToHsl();
    public abstract HsvColor ToHsv();
    public abstract XyzColor ToXyz();
    public abstract LabColor ToLab();
    public abstract LchColor ToLch();
    public abstract OkLabColor ToOkLab();
    public abstract OkLchColor ToOkLch();

    public static bool TryParseCssColor(string value, [NotNullWhen(true)] out BaseColor? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        value = value.Trim().ToLowerInvariant();

        if (value.StartsWith("#"))
            return TryParseHex(value, out color);

        if (value.StartsWith("rgb"))
            return TryParseRgb(value, out color);

        if (value.StartsWith("hsl"))
            return TryParseHsl(value, out color);

        if (value.StartsWith("oklab"))
            return TryParseOklab(value, out color);

        if (value.StartsWith("oklch"))
            return TryParseOklch(value, out color);

        //return TryParseNamed(value, out color);
        return false;
    }

    public static bool TryParseHex(string input, out HexColor? color)
    {
        color = null;
        string hex = input.TrimStart('#');

        try
        {
            if (hex.Length == 3)
            {
                color = new RgbColor(
                    Convert.ToInt32(new string(hex[0], 2), 16),
                    Convert.ToInt32(new string(hex[1], 2), 16),
                    Convert.ToInt32(new string(hex[2], 2), 16)
                );
                return true;
            }

            if (hex.Length == 4)
            {
                color = new RgbColor(
                    Convert.ToInt32(new string(hex[0], 2), 16),
                    Convert.ToInt32(new string(hex[1], 2), 16),
                    Convert.ToInt32(new string(hex[2], 2), 16),
                    Convert.ToInt32(new string(hex[3], 2), 16) / 255.0
                );
                return true;
            }

            if (hex.Length == 6)
            {
                color = new RgbColor(
                    Convert.ToInt32(hex[..2], 16),
                    Convert.ToInt32(hex[2..4], 16),
                    Convert.ToInt32(hex[4..6], 16)
                );
                return true;
            }

            if (hex.Length == 8)
            {
                color = new RgbColor(
                    Convert.ToInt32(hex[..2], 16),
                    Convert.ToInt32(hex[2..4], 16),
                    Convert.ToInt32(hex[4..6], 16),
                    Convert.ToInt32(hex[6..8], 16) / 255.0
                );
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private static bool TryParseRgb(string input, out BaseColor? color)
    {
        color = null;

        try
        {
            string inner = input[input.IndexOf('(') + 1..input.LastIndexOf(')')];
            inner = inner.Replace(",", " ");

            string[] parts = inner.Split('/', StringSplitOptions.TrimEntries);
            string[] rgb = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int r = int.Parse(rgb[0]);
            int g = int.Parse(rgb[1]);
            int b = int.Parse(rgb[2]);

            double a = parts.Length == 2 ? double.Parse(parts[1]) : 1.0;

            color = new RgbColor(r, g, b, a);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseHsl(string input, out BaseColor? color)
    {
        color = null;

        try
        {
            string inner = input[input.IndexOf('(') + 1..input.LastIndexOf(')')];
            inner = inner.Replace(",", " ");

            string[] parts = inner.Split('/', StringSplitOptions.TrimEntries);
            string[] hsl = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int h = int.Parse(hsl[0]);
            int s = int.Parse(hsl[1].TrimEnd('%'));
            int l = int.Parse(hsl[2].TrimEnd('%'));

            double a = parts.Length == 2 ? double.Parse(parts[1]) : 1.0;

            color = new HslColor(h, s, l, a);
            return true;
        }
        catch
        {
            return false;
        }
    }


    private static bool TryParseOklab(string input, out BaseColor? color)
    {
        color = null;

        try
        {
            string inner = input[input.IndexOf('(') + 1..input.LastIndexOf(')')];
            string[] parts = inner.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            double l = double.Parse(parts[0]);
            double a = double.Parse(parts[1]);
            double b = double.Parse(parts[2]);

            double alpha = 1.0;
            if (parts.Length == 5 && parts[3] == "/")
                alpha = double.Parse(parts[4]);

            color = new OkLabColor(l, a, b, alpha);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseOklch(string input, out BaseColor? color)
    {
        color = null;

        try
        {
            string inner = input[input.IndexOf('(') + 1..input.LastIndexOf(')')];
            string[] parts = inner.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            double l = double.Parse(parts[0]);
            double c = double.Parse(parts[1]);
            double h = double.Parse(parts[2]);

            double alpha = 1.0;
            if (parts.Length == 5 && parts[3] == "/")
                alpha = double.Parse(parts[4]);

            color = new OkLchColor(l, c, h, alpha);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

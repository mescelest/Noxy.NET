using System.ComponentModel;
using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Models;

public record PlacementRequest(
    Rect Anchor,
    double PanelWidth,
    double PanelHeight,
    double ViewportWidth,
    double ViewportHeight,
    PlacementSideEnum Side,
    PlacementAlignEnum Align,
    double Gap,
    double Margin)
{
    public PlacementResult Resolve()
    {
        PlacementSideEnum side = ChooseSide();
        double usableHeight = Math.Max(0, ViewportHeight - 2 * Margin);
        double maxHeight = IsVertical(side) ? Math.Min(SpaceOn(side), usableHeight) : usableHeight;
        double height = Math.Min(PanelHeight, maxHeight);

        double x;
        double y;
        switch (side)
        {
            case PlacementSideEnum.Bottom:
                y = ClampY(Anchor.Bottom + Gap, height);
                x = ClampX(CrossStart(Anchor.X, Anchor.Width, PanelWidth));
                break;
            case PlacementSideEnum.Top:
                y = ClampY(Anchor.Y - Gap - height, height);
                x = ClampX(CrossStart(Anchor.X, Anchor.Width, PanelWidth));
                break;
            case PlacementSideEnum.Right:
                x = ClampX(Anchor.Right + Gap);
                y = ClampY(CrossStart(Anchor.Y, Anchor.Height, height), height);
                break;
            case PlacementSideEnum.Left:
                x = ClampX(Anchor.X - Gap - PanelWidth);
                y = ClampY(CrossStart(Anchor.Y, Anchor.Height, height), height);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(side), (int)side, typeof(PlacementSideEnum));
        }

        return new(x, y, maxHeight, side);
    }

    private PlacementSideEnum ChooseSide()
    {
        PlacementSideEnum opposite = Opposite(Side);
        if (Fits(Side))
        {
            return Side;
        }

        if (Fits(opposite))
        {
            return opposite;
        }

        return SpaceOn(opposite) > SpaceOn(Side) ? opposite : Side;
    }

    private bool Fits(PlacementSideEnum side)
    {
        double need = IsVertical(side) ? PanelHeight : PanelWidth;
        return need <= SpaceOn(side);
    }

    private double SpaceOn(PlacementSideEnum side)
    {
        double space = side switch
        {
            PlacementSideEnum.Bottom => ViewportHeight - Margin - (Anchor.Bottom + Gap),
            PlacementSideEnum.Top => Anchor.Y - Gap - Margin,
            PlacementSideEnum.Right => ViewportWidth - Margin - (Anchor.Right + Gap),
            PlacementSideEnum.Left => Anchor.X - Gap - Margin,
            _ => throw new InvalidEnumArgumentException(nameof(side), (int)side, typeof(PlacementSideEnum))
        };
        return Math.Max(0, space);
    }

    private double CrossStart(double anchorStart, double anchorSize, double panelSize)
    {
        return Align switch
        {
            PlacementAlignEnum.Start => anchorStart,
            PlacementAlignEnum.Center => anchorStart + (anchorSize - panelSize) / 2,
            PlacementAlignEnum.End => anchorStart + anchorSize - panelSize,
            _ => throw new InvalidEnumArgumentException(nameof(Align), (int)Align, typeof(PlacementAlignEnum))
        };
    }

    private double ClampX(double x)
    {
        return Clamp(x, PanelWidth, ViewportWidth);
    }

    private double ClampY(double y, double height)
    {
        return Clamp(y, height, ViewportHeight);
    }

    private double Clamp(double start, double size, double viewport)
    {
        double max = viewport - Margin - size;
        return Math.Max(Margin, Math.Min(start, max));
    }

    private static bool IsVertical(PlacementSideEnum side)
    {
        return side is PlacementSideEnum.Bottom or PlacementSideEnum.Top;
    }

    private static PlacementSideEnum Opposite(PlacementSideEnum side)
    {
        return side switch
        {
            PlacementSideEnum.Bottom => PlacementSideEnum.Top,
            PlacementSideEnum.Top => PlacementSideEnum.Bottom,
            PlacementSideEnum.Right => PlacementSideEnum.Left,
            PlacementSideEnum.Left => PlacementSideEnum.Right,
            _ => throw new InvalidEnumArgumentException(nameof(side), (int)side, typeof(PlacementSideEnum))
        };
    }
}

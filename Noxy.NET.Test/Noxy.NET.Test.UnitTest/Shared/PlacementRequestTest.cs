using Noxy.NET.UI.Enums;
using Noxy.NET.UI.Models;

namespace Noxy.NET.Test.Shared;

public class PlacementRequestTest
{
    private const double ViewportWidth = 1000;
    private const double ViewportHeight = 800;
    private const double Gap = 4;
    private const double Margin = 8;

    private static PlacementRequest Request(Rect anchor, double panelWidth = 200, double panelHeight = 300, PlacementSideEnum side = PlacementSideEnum.Bottom, PlacementAlignEnum align = PlacementAlignEnum.Start)
    {
        return new(anchor, panelWidth, panelHeight, ViewportWidth, ViewportHeight, side, align, Gap, Margin);
    }

    [Fact]
    public void Resolve_UsesThePreferredSideWhenItFits()
    {
        PlacementResult result = Request(new(100, 100, 50, 30)).Resolve();

        Assert.Equal(PlacementSideEnum.Bottom, result.Side);
        Assert.Equal(100, result.X);
        Assert.Equal(134, result.Y);
    }

    [Fact]
    public void Resolve_FlipsToTheOppositeSide_WhenThePreferredOneOverflows_AndTheOtherFits()
    {
        PlacementResult result = Request(new(100, 600, 50, 30)).Resolve();

        Assert.Equal(PlacementSideEnum.Top, result.Side);
        Assert.Equal(600 - Gap - 300, result.Y);
    }

    [Fact]
    public void Resolve_PicksTheSideWithMoreRoom_AndCapsMaxHeight_WhenNeitherFits()
    {
        PlacementResult below = Request(new(100, 300, 50, 30), panelHeight: 2000).Resolve();
        Assert.Equal(PlacementSideEnum.Bottom, below.Side);
        Assert.Equal(ViewportHeight - Margin - (330 + Gap), below.MaxHeight);

        PlacementResult above = Request(new(100, 650, 50, 30), panelHeight: 2000).Resolve();
        Assert.Equal(PlacementSideEnum.Top, above.Side);
        Assert.Equal(650 - Gap - Margin, above.MaxHeight);
        Assert.Equal(Margin, above.Y);
    }

    [Fact]
    public void Resolve_ShiftsAlongTheOtherAxis_ToStayInsideTheMargin()
    {
        PlacementResult result = Request(new(ViewportWidth - 20, 100, 20, 30)).Resolve();

        Assert.Equal(ViewportWidth - Margin - 200, result.X);
    }

    [Fact]
    public void Resolve_AlignsStartCenterAndEnd()
    {
        Rect anchor = new(400, 100, 100, 30);

        Assert.Equal(400, Request(anchor, align: PlacementAlignEnum.Start).Resolve().X);
        Assert.Equal(350, Request(anchor, align: PlacementAlignEnum.Center).Resolve().X);
        Assert.Equal(300, Request(anchor, align: PlacementAlignEnum.End).Resolve().X);
    }

    [Fact]
    public void Resolve_ForAPoint_OpensBelowAndRight_AndFlipsUpAndLeftInTheBottomRightCorner()
    {
        PlacementResult open = Request(new(100, 100, 0, 0)).Resolve();
        Assert.Equal(PlacementSideEnum.Bottom, open.Side);
        Assert.Equal(100, open.X);
        Assert.True(open.Y > 100);

        PlacementResult corner = Request(new(ViewportWidth - 2, ViewportHeight - 2, 0, 0)).Resolve();
        Assert.Equal(PlacementSideEnum.Top, corner.Side);
        Assert.True(corner.Y + 300 <= ViewportHeight - 2);
        Assert.True(corner.X + 200 <= ViewportWidth - Margin);
        Assert.True(corner.X < ViewportWidth - 2);
    }

    [Fact]
    public void Resolve_ForSideRight_FlipsToLeft_WhenThereIsNoRoomOnTheRight()
    {
        PlacementResult fits = Request(new(100, 100, 150, 30), side: PlacementSideEnum.Right).Resolve();
        Assert.Equal(PlacementSideEnum.Right, fits.Side);
        Assert.Equal(250 + Gap, fits.X);
        Assert.Equal(100, fits.Y);

        PlacementResult flipped = Request(new(700, 100, 280, 30), side: PlacementSideEnum.Right).Resolve();
        Assert.Equal(PlacementSideEnum.Left, flipped.Side);
        Assert.Equal(700 - Gap - 200, flipped.X);

        PlacementResult shifted = Request(new(100, 700, 150, 30), side: PlacementSideEnum.Right).Resolve();
        Assert.Equal(ViewportHeight - Margin - 300, shifted.Y);
    }

    [Theory]
    [InlineData(0, 0, 200, 300)]
    [InlineData(ViewportWidth, 0, 200, 300)]
    [InlineData(0, ViewportHeight, 200, 300)]
    [InlineData(ViewportWidth, ViewportHeight, 200, 300)]
    [InlineData(500, 400, 600, 700)]
    [InlineData(ViewportWidth, ViewportHeight, 900, 2000)]
    public void Resolve_NeverReturnsAPositionOutsideTheMargin(double anchorX, double anchorY, double panelWidth, double panelHeight)
    {
        foreach (PlacementSideEnum side in Enum.GetValues<PlacementSideEnum>())
        {
            foreach (PlacementAlignEnum align in Enum.GetValues<PlacementAlignEnum>())
            {
                PlacementResult result = Request(new(anchorX, anchorY, 0, 0), panelWidth, panelHeight, side, align).Resolve();
                double height = Math.Min(panelHeight, result.MaxHeight);

                Assert.True(result.X >= Margin - 0.001, $"{side}/{align} X {result.X}");
                Assert.True(result.Y >= Margin - 0.001, $"{side}/{align} Y {result.Y}");
                Assert.True(result.X + panelWidth <= ViewportWidth - Margin + 0.001, $"{side}/{align} right {result.X + panelWidth}");
                Assert.True(result.Y + height <= ViewportHeight - Margin + 0.001, $"{side}/{align} bottom {result.Y + height}");
            }
        }
    }
}

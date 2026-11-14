using Noxy.NET.MediaPlayer.Domain.Requests;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class OptionalRangeAttributeTest
{
    private readonly OptionalRangeAttribute _attribute = new(0, 9999);

    [Fact]
    public void LeftOut_IsValid()
    {
        Assert.True(_attribute.IsValid(default(Optional<int?>)));
    }

    [Fact]
    public void NoValueAtAll_IsValid()
    {
        Assert.True(_attribute.IsValid(null));
    }

    [Fact]
    public void PresentNull_IsValid()
    {
        Assert.True(_attribute.IsValid(new Optional<int?>(null)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1999)]
    [InlineData(9999)]
    public void InsideTheRange_IsValid(int number)
    {
        Assert.True(_attribute.IsValid(new Optional<int?>(number)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10000)]
    public void OutsideTheRange_IsInvalid(int number)
    {
        Assert.False(_attribute.IsValid(new Optional<int?>(number)));
    }

    [Fact]
    public void Message_NamesTheFieldAndTheRange()
    {
        Assert.Equal("The field Year must be between 0 and 9999.", _attribute.FormatErrorMessage("Year"));
    }
}

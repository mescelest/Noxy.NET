using Noxy.NET.MediaPlayer.Domain.Requests;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class OptionalNotBlankAttributeTest
{
    private readonly OptionalNotBlankAttribute _attribute = new();

    [Fact]
    public void LeftOut_IsValid()
    {
        Assert.True(_attribute.IsValid(default(Optional<string?>)));
    }

    [Fact]
    public void NoValueAtAll_IsValid()
    {
        Assert.True(_attribute.IsValid(null));
    }

    [Theory]
    [InlineData("x")]
    [InlineData(" padded ")]
    public void Text_IsValid(string text)
    {
        Assert.True(_attribute.IsValid(new Optional<string?>(text)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void PresentButBlankOrNull_IsInvalid(string? text)
    {
        Assert.False(_attribute.IsValid(new Optional<string?>(text)));
    }

    [Fact]
    public void Message_NamesTheField()
    {
        Assert.Equal("The field Title can't be empty.", _attribute.FormatErrorMessage("Title"));
    }
}

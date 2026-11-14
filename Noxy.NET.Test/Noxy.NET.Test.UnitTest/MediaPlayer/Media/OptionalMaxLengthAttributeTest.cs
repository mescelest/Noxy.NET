using Noxy.NET.MediaPlayer.Domain.Requests;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class OptionalMaxLengthAttributeTest
{
    private readonly OptionalMaxLengthAttribute _attribute = new(5);

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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("abcde")]
    public void AtOrUnderTheLimit_IsValid(string? text)
    {
        Assert.True(_attribute.IsValid(new Optional<string?>(text)));
    }

    [Fact]
    public void OverTheLimit_IsInvalid()
    {
        Assert.False(_attribute.IsValid(new Optional<string?>("abcdef")));
    }

    [Fact]
    public void Message_NamesTheFieldAndTheLimit()
    {
        Assert.Equal("The field Artist can be at most 5 characters.", _attribute.FormatErrorMessage("Artist"));
    }
}

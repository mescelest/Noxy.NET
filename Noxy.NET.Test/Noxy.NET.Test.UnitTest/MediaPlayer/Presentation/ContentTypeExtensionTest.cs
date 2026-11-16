using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ContentTypeExtensionTest
{
    [Theory]
    [InlineData("audio/mpeg", ".mp3")]
    [InlineData("AUDIO/MPEG", ".mp3")]
    [InlineData("audio/ogg; codecs=opus", ".ogg")]
    [InlineData("  audio/flac  ", ".flac")]
    [InlineData("audio/mp4", ".m4a")]
    [InlineData("audio/x-m4a", ".m4a")]
    [InlineData("audio/wav", ".wav")]
    [InlineData("audio/x-wav", ".wav")]
    [InlineData("application/octet-stream", ".bin")]
    [InlineData("", ".bin")]
    public void TheFileExtension_FollowsTheMediaType_IgnoringCaseAndParameters(string contentType, string expected)
    {
        Assert.Equal(expected, contentType.GetFileExtension());
    }
}

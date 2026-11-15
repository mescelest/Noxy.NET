using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Validators;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Storage;

public class MediaFileValidatorTest
{
    private static readonly string[] All = ["mp3", "flac", "m4a", "ogg", "opus", "wav"];

    private static readonly byte[] Mp3WithTag = [.. "ID3"u8, 3, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] Mp3Frame = [0xFF, 0xFB, 0x90, 0x00, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] Flac = [.. "fLaC"u8, 0, 0, 0, 0x22, 0, 0, 0, 0];
    private static readonly byte[] M4A = [0, 0, 0, 0x20, .. "ftypM4A "u8];
    private static readonly byte[] Ogg = [.. "OggS"u8, 0, 2, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] Wav = [.. "RIFF"u8, 1, 2, 3, 4, .. "WAVE"u8];

    private static MediaFileValidator Create(params string[] allowed)
    {
        return new(Options.Create(new ConfigStorage { RootPath = "x", AllowedExtensionList = allowed }));
    }

    [Theory]
    [InlineData("media.mp3", "mp3", "audio/mpeg")]
    [InlineData("media.FLAC", "flac", "audio/flac")]
    [InlineData("media.m4a", "m4a", "audio/mp4")]
    [InlineData("media.ogg", "ogg", "audio/ogg")]
    [InlineData("media.opus", "opus", "audio/ogg")]
    [InlineData("media.wav", "wav", "audio/wav")]
    [InlineData("media.test.mp3", "mp3", "audio/mpeg")]
    public void Validate_MatchingExtensionAndHeader_ReturnsNormalizedFile(string fileName, string extension, string contentType)
    {
        byte[] header = extension switch
        {
            "mp3" => Mp3WithTag,
            "flac" => Flac,
            "m4a" => M4A,
            "wav" => Wav,
            _ => Ogg
        };

        ValidatedMediaFile result = Create(All).Validate(fileName, header);

        Assert.Equal(new(extension, contentType), result);
    }

    [Fact]
    public void Validate_Mp3FrameSyncWithoutTag_IsAccepted()
    {
        Assert.Equal("mp3", Create(All).Validate("media.mp3", Mp3Frame).Extension);
    }

    [Theory]
    [InlineData("media.mp3.exe")]
    [InlineData("media.exe")]
    [InlineData("media")]
    [InlineData("media.")]
    public void Validate_ExtensionNotAllowed_Throws(string fileName)
    {
        Assert.Throws<UnsupportedMediaTypeException>(() => Create(All).Validate(fileName, Mp3WithTag));
    }

    [Fact]
    public void Validate_ExtensionNotInConfiguredList_Throws()
    {
        Assert.Throws<UnsupportedMediaTypeException>(() => Create("flac").Validate("media.mp3", Mp3WithTag));
    }

    [Fact]
    public void Validate_AllowedExtensionWithoutKnownFormat_Throws()
    {
        Assert.Throws<UnsupportedMediaTypeException>(() => Create("aiff").Validate("media.aiff", Mp3WithTag));
    }

    [Fact]
    public void Validate_HeaderOfAnotherFormat_Throws()
    {
        Assert.Throws<UnsupportedMediaTypeException>(() => Create(All).Validate("media.mp3", Flac));
    }

    [Fact]
    public void Validate_RiffThatIsNotWave_Throws()
    {
        byte[] avi = [.. "RIFF"u8, 1, 2, 3, 4, .. "AVI "u8];

        Assert.Throws<UnsupportedMediaTypeException>(() => Create(All).Validate("media.wav", avi));
    }

    [Fact]
    public void Validate_HeaderTooShort_Throws()
    {
        Assert.Throws<UnsupportedMediaTypeException>(() => Create(All).Validate("media.m4a", [0, 0, 0]));
        Assert.Throws<UnsupportedMediaTypeException>(() => Create(All).Validate("media.mp3", []));
    }

    [Fact]
    public void Validate_ConfiguredExtensionWithDotAndCase_IsNormalized()
    {
        Assert.Equal("mp3", Create(".MP3").Validate("media.mp3", Mp3WithTag).Extension);
    }
}

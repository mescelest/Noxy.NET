using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Storage;

public class ConfigStorageTest
{
    private static ConfigStorage Valid()
    {
        return new() { RootPath = "Media", AllowedExtensionList = ["mp3"] };
    }

    private static bool IsValid(ConfigStorage config)
    {
        return Validator.TryValidateObject(config, new(config), [], true);
    }

    [Fact]
    public void Valid_Passes()
    {
        Assert.True(IsValid(Valid()));
    }

    [Fact]
    public void DefaultMaxSize_Is100Megabytes()
    {
        Assert.Equal(100L * 1024 * 1024, Valid().MaxSizeBytes);
    }

    [Fact]
    public void EmptyRootPath_Fails()
    {
        ConfigStorage config = Valid();
        config.RootPath = "";

        Assert.False(IsValid(config));
    }

    [Fact]
    public void NoAllowedExtensions_Fails()
    {
        ConfigStorage config = Valid();
        config.AllowedExtensionList = [];

        Assert.False(IsValid(config));
    }

    [Fact]
    public void DefaultLimits_Are5000FilesAndNoByteLimit()
    {
        ConfigStorage config = Valid();

        Assert.Equal(5000, config.MaxFilesPerUser);
        Assert.Equal(0, config.MaxBytesPerUser);
    }

    [Fact]
    public void NonPositiveMaxFiles_Fails()
    {
        ConfigStorage config = Valid();
        config.MaxFilesPerUser = 0;

        Assert.False(IsValid(config));
    }

    [Fact]
    public void NegativeMaxBytes_Fails()
    {
        ConfigStorage config = Valid();
        config.MaxBytesPerUser = -1;

        Assert.False(IsValid(config));
    }

    [Fact]
    public void NonPositiveMaxSize_Fails()
    {
        ConfigStorage config = Valid();
        config.MaxSizeBytes = 0;

        Assert.False(IsValid(config));
    }

    [Fact]
    public void DefaultFolderNames_AreLibraryStagingTemporaryAndAlbumArt()
    {
        ConfigStorage config = Valid();

        Assert.Equal("library", config.LibraryFolderName);
        Assert.Equal("staging", config.StagingFolderName);
        Assert.Equal("temporary", config.TemporaryFolderName);
        Assert.Equal("albumart", config.AlbumArtFolderName);
    }

    [Fact]
    public void CustomFolderNames_ThatDiffer_Pass()
    {
        ConfigStorage config = Valid();
        config.LibraryFolderName = "files";
        config.StagingFolderName = "incoming";
        config.TemporaryFolderName = "scratch";
        config.AlbumArtFolderName = "covers";

        Assert.True(IsValid(config));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("a/b")]
    [InlineData("a\b")]
    [InlineData("../outside")]
    [InlineData("C:")]
    [InlineData("-leading")]
    public void ALibraryFolderName_ThatIsNotAPlainFolderName_Fails(string name)
    {
        ConfigStorage config = Valid();
        config.LibraryFolderName = name;

        Assert.False(IsValid(config));
    }

    [Theory]
    [InlineData(nameof(ConfigStorage.StagingFolderName))]
    [InlineData(nameof(ConfigStorage.TemporaryFolderName))]
    [InlineData(nameof(ConfigStorage.AlbumArtFolderName))]
    public void EveryFolderName_IsChecked_NotJustTheLibraryOne(string property)
    {
        ConfigStorage config = Valid();
        typeof(ConfigStorage).GetProperty(property)?.SetValue(config, "a/b");

        Assert.False(IsValid(config));
    }

    [Theory]
    [InlineData("library", "library", "temporary", "albumart")]
    [InlineData("library", "staging", "library", "albumart")]
    [InlineData("library", "staging", "temporary", "library")]
    [InlineData("library", "staging", "staging", "albumart")]
    [InlineData("library", "staging", "temporary", "TEMPORARY")]
    public void FolderNames_ThatAreTheSameFolder_Fail(string library, string staging, string temporary, string albumArt)
    {
        ConfigStorage config = Valid();
        config.LibraryFolderName = library;
        config.StagingFolderName = staging;
        config.TemporaryFolderName = temporary;
        config.AlbumArtFolderName = albumArt;

        Assert.False(IsValid(config));
    }
}

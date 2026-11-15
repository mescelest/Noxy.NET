using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingCatalogTest
{
    [Fact]
    public void TheFourSettingsAreDeclaredInOrder_WithTheirKeys()
    {
        Assert.Equal([SettingKey.Shuffle, SettingKey.Repeat, SettingKey.Volume, SettingKey.MergeOnRename], SettingCatalog.DeclarationList.Select(x => x.Key).ToList());
        Assert.Equal(["Shuffle", "Repeat", "Volume", "MergeOnRename"], SettingCatalog.DeclarationList.Select(x => x.Key).ToList());
    }

    [Fact]
    public void TheKeysAreUnique()
    {
        Assert.Equal(SettingCatalog.DeclarationList.Count, SettingCatalog.DeclarationList.Select(x => x.Key).Distinct().Count());
    }

    [Fact]
    public void EveryDefault_IsAcceptedByItsOwnDeclaration()
    {
        foreach (SettingDeclaration declaration in SettingCatalog.DeclarationList)
        {
            JsonElement element = JsonSerializer.SerializeToElement(declaration.DefaultValue);

            Assert.Equal(declaration.DefaultValue, declaration.ParseValue(element));
        }
    }

    [Fact]
    public void TheDeclaredTypesAndDefaults_AreTheOnesOfTheDesign()
    {
        Assert.Equal(SettingValueType.Enum, SettingCatalog.Shuffle.ValueType);
        Assert.Equal(nameof(ShuffleMode.Off), SettingCatalog.Shuffle.DefaultValue);
        Assert.Equal([nameof(ShuffleMode.Off), nameof(ShuffleMode.Random), nameof(ShuffleMode.SpreadArtists)], SettingCatalog.Shuffle.MemberNameList);
        Assert.Equal(SettingValueType.Enum, SettingCatalog.Repeat.ValueType);
        Assert.Equal(nameof(RepeatMode.No), SettingCatalog.Repeat.DefaultValue);
        Assert.Equal([nameof(RepeatMode.No), nameof(RepeatMode.Once), nameof(RepeatMode.Forever)], SettingCatalog.Repeat.MemberNameList);
        Assert.Equal(SettingValueType.Integer, SettingCatalog.Volume.ValueType);
        Assert.Equal(100, SettingCatalog.Volume.DefaultValue);
        Assert.Equal((0, 100), (SettingCatalog.Volume.Minimum, SettingCatalog.Volume.Maximum));
        Assert.Equal(SettingValueType.Boolean, SettingCatalog.MergeOnRename.ValueType);
        Assert.Equal(false, SettingCatalog.MergeOnRename.DefaultValue);
    }

    [Fact]
    public void Find_ReturnsTheDeclarationOfAKey_AndIsCaseSensitive()
    {
        Assert.Same(SettingCatalog.Volume, SettingCatalog.Find("Volume"));
        Assert.Same(SettingCatalog.MergeOnRename, SettingCatalog.Find(SettingKey.MergeOnRename));
        Assert.Null(SettingCatalog.Find("volume"));
        Assert.Null(SettingCatalog.Find("Nope"));
        Assert.Null(SettingCatalog.Find(string.Empty));
    }
}

using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingCatalogTest
{
    [Fact]
    public void TheFiveSettingsAreDeclaredInOrder_WithTheirKeys()
    {
        Assert.Equal([SettingKey.Shuffle, SettingKey.Repeat, SettingKey.MergeOnRename, SettingKey.KeepQueuedItems, SettingKey.PrefetchCount], SettingCatalog.DeclarationList.Select(x => x.Key).ToList());
        Assert.Equal(["Shuffle", "Repeat", "MergeOnRename", "KeepQueuedItems", "PrefetchCount"], SettingCatalog.DeclarationList.Select(x => x.Key).ToList());
    }

    [Fact]
    public void PrefetchCount_IsAnIntegerFromZeroToTen_DefaultThree()
    {
        Assert.Equal(SettingValueTypeEnum.Integer, SettingCatalog.PrefetchCount.ValueType);
        Assert.Equal((3, 0, 10), (SettingCatalog.PrefetchCount.Default, SettingCatalog.PrefetchCount.Minimum, SettingCatalog.PrefetchCount.Maximum));
        Assert.Equal(7, SettingCatalog.PrefetchCount.ParseValue(JsonSerializer.SerializeToElement(7)));
        Assert.ThrowsAny<Exception>(() => SettingCatalog.PrefetchCount.ParseValue(JsonSerializer.SerializeToElement(11)));
        Assert.ThrowsAny<Exception>(() => SettingCatalog.PrefetchCount.ParseValue(JsonSerializer.SerializeToElement(-1)));
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
        Assert.Equal(SettingValueTypeEnum.Enum, SettingCatalog.Shuffle.ValueType);
        Assert.Equal(nameof(ShuffleModeEnum.Off), SettingCatalog.Shuffle.DefaultValue);
        Assert.Equal([nameof(ShuffleModeEnum.Off), nameof(ShuffleModeEnum.Random), nameof(ShuffleModeEnum.SpreadArtists)], SettingCatalog.Shuffle.MemberNameList);
        Assert.Equal(SettingValueTypeEnum.Enum, SettingCatalog.Repeat.ValueType);
        Assert.Equal(nameof(RepeatModeEnum.No), SettingCatalog.Repeat.DefaultValue);
        Assert.Equal([nameof(RepeatModeEnum.No), nameof(RepeatModeEnum.Once), nameof(RepeatModeEnum.Forever)], SettingCatalog.Repeat.MemberNameList);
        Assert.Equal(SettingValueTypeEnum.Boolean, SettingCatalog.MergeOnRename.ValueType);
        Assert.Equal(false, SettingCatalog.MergeOnRename.DefaultValue);
    }

    [Fact]
    public void Catalog_ContainsKeepQueuedItems_DefaultFalse()
    {
        Assert.Contains(SettingCatalog.KeepQueuedItems, SettingCatalog.DeclarationList);
        Assert.Equal(SettingValueTypeEnum.Boolean, SettingCatalog.KeepQueuedItems.ValueType);
        Assert.Equal(false, SettingCatalog.KeepQueuedItems.DefaultValue);
        Assert.Equal("KeepQueuedItems", SettingKey.KeepQueuedItems);
    }

    [Fact]
    public void Find_IsCaseExact_ForKeepQueuedItems()
    {
        Assert.Same(SettingCatalog.KeepQueuedItems, SettingCatalog.Find("KeepQueuedItems"));
        Assert.Null(SettingCatalog.Find("keepQueuedItems"));
        Assert.Null(SettingCatalog.Find("KEEPQUEUEDITEMS"));
    }

    [Fact]
    public void Find_ReturnsTheDeclarationOfAKey_AndIsCaseSensitive()
    {
        Assert.Same(SettingCatalog.Shuffle, SettingCatalog.Find("Shuffle"));
        Assert.Same(SettingCatalog.MergeOnRename, SettingCatalog.Find(SettingKey.MergeOnRename));
        Assert.Null(SettingCatalog.Find("shuffle"));
        Assert.Null(SettingCatalog.Find("Volume"));
        Assert.Null(SettingCatalog.Find("Nope"));
        Assert.Null(SettingCatalog.Find(string.Empty));
    }
}

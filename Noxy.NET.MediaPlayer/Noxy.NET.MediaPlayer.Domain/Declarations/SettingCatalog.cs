using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public static class SettingCatalog
{
    public static readonly SettingDeclarationEnumOf<ShuffleModeEnum> Shuffle = new(SettingKey.Shuffle, ShuffleModeEnum.Off);
    public static readonly SettingDeclarationEnumOf<RepeatModeEnum> Repeat = new(SettingKey.Repeat, RepeatModeEnum.No);
    public static readonly SettingDeclarationBoolean MergeOnRename = new(SettingKey.MergeOnRename, false);
    public static readonly SettingDeclarationBoolean KeepQueuedItems = new(SettingKey.KeepQueuedItems, false);

    // How many queue entries after the current one have their files downloaded ahead of time; 0 turns it off.
    public static readonly SettingDeclarationInteger PrefetchCount = new(SettingKey.PrefetchCount, 3, 0, 10);

    // Whether a device writes the current metadata into the files it holds when the metadata changes, and at start.
    public static readonly SettingDeclarationBoolean UpdateFileMetadata = new(SettingKey.UpdateFileMetadata, false);

    public static readonly IReadOnlyList<SettingDeclaration> DeclarationList = [Shuffle, Repeat, MergeOnRename, KeepQueuedItems, PrefetchCount, UpdateFileMetadata];

    // The key is exact: "shuffle" is not "Shuffle".
    public static SettingDeclaration? Find(string key)
    {
        return DeclarationList.FirstOrDefault(x => x.Key == key);
    }
}

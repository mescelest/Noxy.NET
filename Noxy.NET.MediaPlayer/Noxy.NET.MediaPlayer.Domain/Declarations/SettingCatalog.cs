using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public static class SettingCatalog
{
    public static readonly SettingDeclarationEnumOf<ShuffleModeEnum> Shuffle = new(SettingKey.Shuffle, ShuffleModeEnum.Off);
    public static readonly SettingDeclarationEnumOf<RepeatModeEnum> Repeat = new(SettingKey.Repeat, RepeatModeEnum.No);
    public static readonly SettingDeclarationBoolean MergeOnRename = new(SettingKey.MergeOnRename, false);
    public static readonly SettingDeclarationBoolean KeepQueuedItems = new(SettingKey.KeepQueuedItems, false);

    public static readonly IReadOnlyList<SettingDeclaration> DeclarationList = [Shuffle, Repeat, MergeOnRename, KeepQueuedItems];

    // The key is exact: "shuffle" is not "Shuffle".
    public static SettingDeclaration? Find(string key)
    {
        return DeclarationList.FirstOrDefault(x => x.Key == key);
    }
}

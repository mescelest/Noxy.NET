using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public static class SettingCatalog
{
    public static readonly SettingDeclarationEnumOf<ShuffleModeEnum> Shuffle = new(SettingKey.Shuffle, ShuffleModeEnum.Off);
    public static readonly SettingDeclarationEnumOf<RepeatModeEnum> Repeat = new(SettingKey.Repeat, RepeatModeEnum.No);
    public static readonly SettingDeclarationInteger Volume = new(SettingKey.Volume, 100, 0, 100);
    public static readonly SettingDeclarationBoolean MergeOnRename = new(SettingKey.MergeOnRename, false);

    public static readonly IReadOnlyList<SettingDeclaration> DeclarationList = [Shuffle, Repeat, Volume, MergeOnRename];

    // The key is exact: "volume" is not "Volume".
    public static SettingDeclaration? Find(string key)
    {
        return DeclarationList.FirstOrDefault(x => x.Key == key);
    }
}

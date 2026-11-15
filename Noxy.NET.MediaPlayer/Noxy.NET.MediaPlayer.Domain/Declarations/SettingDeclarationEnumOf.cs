namespace Noxy.NET.MediaPlayer.Domain.Declarations;

public record SettingDeclarationEnumOf<TEnum>(string Key, TEnum Default) : SettingDeclarationEnum(Key, Enum.GetNames<TEnum>(), Default.ToString()) where TEnum : struct, Enum;

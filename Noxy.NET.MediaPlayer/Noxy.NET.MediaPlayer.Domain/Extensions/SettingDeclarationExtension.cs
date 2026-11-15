using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class SettingDeclarationExtension
{
    extension(SettingDeclaration declaration)
    {
        // The value of the setting for a user: the saved row, or the default dated at the start of time when nothing is saved.
        public ResponseSetting ToResponse(EntityUserSetting? saved)
        {
            return new()
            {
                Key = declaration.Key,
                ValueType = declaration.ValueType,
                Value = declaration.ReadValue(saved),
                TimeModified = saved is null ? DateTime.UnixEpoch : DateTime.SpecifyKind(saved.TimeModified, DateTimeKind.Utc)
            };
        }
    }
}

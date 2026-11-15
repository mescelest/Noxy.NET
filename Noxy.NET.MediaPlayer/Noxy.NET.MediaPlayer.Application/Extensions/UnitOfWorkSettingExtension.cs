using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.MediaPlayer.Application.Extensions;

public static class UnitOfWorkSettingExtension
{
    extension(IUnitOfWork uow)
    {
        // The saved value of the user, or the default when there is no row or it is no longer valid.
        public async Task<bool> GetBoolean(Guid userID, SettingDeclarationBoolean declaration, CancellationToken cancellationToken = default)
        {
            return (bool)declaration.ReadValue(await uow.Setting.FindSetting(userID, declaration.Key, cancellationToken));
        }

        public async Task<int> GetInteger(Guid userID, SettingDeclarationInteger declaration, CancellationToken cancellationToken = default)
        {
            return (int)declaration.ReadValue(await uow.Setting.FindSetting(userID, declaration.Key, cancellationToken));
        }

        public async Task<TEnum> GetEnum<TEnum>(Guid userID, SettingDeclarationEnumOf<TEnum> declaration, CancellationToken cancellationToken = default) where TEnum : struct, Enum
        {
            return Enum.Parse<TEnum>((string)declaration.ReadValue(await uow.Setting.FindSetting(userID, declaration.Key, cancellationToken)));
        }
    }
}

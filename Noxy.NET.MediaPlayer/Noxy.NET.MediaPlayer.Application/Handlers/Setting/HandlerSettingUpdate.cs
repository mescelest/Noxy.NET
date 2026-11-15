using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Setting;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Notifications.Setting;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Setting;

public class HandlerSettingUpdate(IUnitOfWorkFactory factoryUnitOfWork, IPublisher publisher, TimeProvider time) : ICommandHandler<CommandSettingUpdate, ResponseSetting>
{
    // Throws SettingNotFoundException for a key that is not declared (the key is exact) and InvalidSettingValueException for a value that does not fit the declaration. The first change of a setting creates its row; a change that leaves the value as it is saves and publishes nothing, also for the default of a user with no row.
    public async ValueTask<ResponseSetting> Handle(CommandSettingUpdate command, CancellationToken cancellationToken)
    {
        SettingDeclaration declaration = SettingCatalog.Find(command.Key) ?? throw new SettingNotFoundException();
        object value = declaration.ParseValue(command.Value);

        // Another device may create the same row first: then the commit conflicts and this runs again as an update.
        (ResponseSetting response, bool changed) = await factoryUnitOfWork.RunWithKeyConflictRetry(uow => Apply(uow, command.UserID, declaration, value, cancellationToken), cancellationToken);

        if (changed) await publisher.Publish(new NotificationSettingUpdated(command.UserID, response), cancellationToken);
        return response;
    }

    private async Task<(ResponseSetting Response, bool Changed)> Apply(IUnitOfWork uow, Guid userID, SettingDeclaration declaration, object value, CancellationToken cancellationToken)
    {
        EntityUserSetting? saved = await uow.Setting.FindSetting(userID, declaration.Key, cancellationToken);
        DateTime now = time.GetUtcNow().UtcDateTime;

        if (saved is null)
        {
            if (value.Equals(declaration.DefaultValue)) return (declaration.ToResponse(null), false);

            saved = declaration.CreateEntity(userID, value);
            saved.TimeModified = now;
            uow.Setting.AddSetting(saved);
        }
        else
        {
            if (!declaration.SetValue(saved, value)) return (declaration.ToResponse(saved), false);

            saved.TimeModified = now;
            uow.Setting.UpdateSetting(saved);
        }

        await uow.Commit(cancellationToken);
        return (declaration.ToResponse(saved), true);
    }
}

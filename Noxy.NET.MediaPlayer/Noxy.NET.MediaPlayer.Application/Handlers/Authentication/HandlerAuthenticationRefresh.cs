using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Authentication;

public class HandlerAuthenticationRefresh(IUnitOfWorkFactory factoryUnitOfWork, ITokenService serviceToken, TimeProvider time) : BaseHandlerAuthentication<CommandAuthenticationRefresh>(factoryUnitOfWork, serviceToken)
{
    public override async ValueTask<ResponseAuthentication> Handle(CommandAuthenticationRefresh command, CancellationToken cancellationToken)
    {
        DateTime now = time.GetUtcNow().UtcDateTime;

        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityRefreshToken stored = await uow.Authentication.FindRefreshToken(ServiceToken.HashRefreshToken(command.RefreshToken), cancellationToken)
                                    ?? throw new AuthenticationFailedException();

        // Tokens are single-use, so a revoked one coming back means someone has a copy.
        // Revoke everything this user has and make them sign in again.
        if (stored.IsRevoked)
        {
            await uow.Authentication.RevokeAllRefreshTokens(stored.UserID, now, cancellationToken);
            throw new AuthenticationFailedException();
        }

        if (stored.IsExpired(now))
        {
            throw new AuthenticationFailedException();
        }

        // Losing this race to another request using the same token counts as reuse too.
        if (!await uow.Authentication.TryRevokeRefreshToken(stored.ID, now, cancellationToken))
        {
            await uow.Authentication.RevokeAllRefreshTokens(stored.UserID, now, cancellationToken);
            throw new AuthenticationFailedException();
        }

        EntityUser user = await uow.Authentication.FindUserByID(stored.UserID, cancellationToken) ?? throw new AuthenticationFailedException();
        ResponseAuthentication response = CreateTokenResponse(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }
}

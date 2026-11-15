using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Authentication;

public class HandlerAuthenticationSignIn(IUnitOfWorkFactory factoryUnitOfWork, IPasswordHashService servicePasswordHash, ITokenService serviceToken) : BaseHandlerAuthentication<CommandAuthenticationSignIn>(factoryUnitOfWork, serviceToken)
{
    public override async ValueTask<ResponseAuthentication> Handle(CommandAuthenticationSignIn command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        EntityUser? user = await uow.Authentication.FindUserByEmail(NormalizeEmail(command.Email), cancellationToken);

        // Verify even when there is no user, so an unknown email takes as long as a wrong password.
        bool isPasswordValid = servicePasswordHash.Verify(command.Password, user?.PasswordHash);
        if (user is null || !isPasswordValid) throw new AuthenticationFailedException();

        ResponseAuthentication response = CreateTokenResponse(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }
}

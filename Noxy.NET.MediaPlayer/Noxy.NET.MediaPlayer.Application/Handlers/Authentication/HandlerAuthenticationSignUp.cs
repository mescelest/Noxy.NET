using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Authentication;

public class HandlerAuthenticationSignUp(IUnitOfWorkFactory factoryUnitOfWork, IPasswordHashService servicePasswordHash, ITokenService serviceToken) : BaseHandlerAuthentication<CommandAuthenticationSignUp>(factoryUnitOfWork, serviceToken)
{
    public override async ValueTask<ResponseAuthentication> Handle(CommandAuthenticationSignUp command, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        string email = NormalizeEmail(command.Email);
        if (await uow.Authentication.EmailExists(email, cancellationToken)) throw new EmailAlreadyInUseException();

        EntityUser user = new()
        {
            Email = email,
            PasswordHash = servicePasswordHash.Hash(command.Password)
        };
        uow.Authentication.AddUser(user);

        ResponseAuthentication response = CreateTokenResponse(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }
}

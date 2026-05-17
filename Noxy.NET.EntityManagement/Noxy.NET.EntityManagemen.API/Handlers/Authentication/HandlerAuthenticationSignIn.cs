using Mediator;
using Noxy.NET.EntityManagement.API.Commands.Authentication;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Handlers.Authentication;

public class HandlerAuthenticationSignIn(IUnitOfWorkFactory serviceUoWFactory, IJWTService serviceJWT) : ICommandHandler<CommandAuthenticationSignIn, ResponseAuthenticationSignIn>
{
    public async ValueTask<ResponseAuthenticationSignIn> Handle(CommandAuthenticationSignIn request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser = await uow.Authentication.GetUserWithEmailAndPassword(request.Email, request.Password);
        return new() { JWT = serviceJWT.Create(entityUser) };
    }
}

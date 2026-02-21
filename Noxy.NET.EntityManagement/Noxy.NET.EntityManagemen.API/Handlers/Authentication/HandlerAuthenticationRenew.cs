using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Authentication;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Handlers.Authentication;

public class HandlerAuthenticationRenew(IUnitOfWorkFactory serviceUoWFactory, IJWTService serviceJWT) : IRequestHandler<CommandAuthenticationRenew, ResponseAuthenticationRenew>
{
    public async Task<ResponseAuthenticationRenew> Handle(CommandAuthenticationRenew request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityUser entityUser = await uow.Authentication.GetUserWithEmail(request.Email);
        entityUser.TimeSignIn = DateTime.UtcNow;

        uow.Authentication.UpdateUser(entityUser);
        await uow.Commit();

        return new() { JWT = serviceJWT.Create(entityUser) };
    }
}

using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Authentication;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Handlers.Authentication;

public class HandlerAuthenticationSignUp(IUnitOfWorkFactory serviceUoWFactory, IJWTService serviceJWT) : IRequestHandler<CommandAuthenticationSignUp, ResponseAuthenticationSignUp>
{
    public async Task<ResponseAuthenticationSignUp> Handle(CommandAuthenticationSignUp request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser = await uow.Authentication.CreateUser(request.Email, request.Password);
        await uow.Commit();
        return new() { JWT = serviceJWT.Create(entityUser) };
    }
}

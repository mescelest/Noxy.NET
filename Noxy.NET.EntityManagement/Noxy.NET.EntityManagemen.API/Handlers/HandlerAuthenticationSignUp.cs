using MediatR;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Handlers;

public class HandlerAuthenticationSignUp(IUnitOfWorkFactory serviceUoWFactory, IJWTService serviceJWT) : IRequestHandler<QueryAuthenticationSignUp, ResponseAuthenticationSignUp>
{
    public async Task<ResponseAuthenticationSignUp> Handle(QueryAuthenticationSignUp request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser = await uow.Authentication.CreateUser(request.Email, request.Password);
        await uow.Commit();
        return new() { JWT = serviceJWT.Create(entityUser) };
    }
}

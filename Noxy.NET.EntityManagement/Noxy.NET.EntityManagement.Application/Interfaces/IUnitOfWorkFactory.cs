namespace Noxy.NET.EntityManagement.Application.Interfaces;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> Create();
}

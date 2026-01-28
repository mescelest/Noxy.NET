namespace Noxy.NET.CaseManagement.Application.Interfaces;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> Create();
}

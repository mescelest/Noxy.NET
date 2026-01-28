using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

namespace Noxy.NET.EntityManagement.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAuthenticationRepository Authentication { get; }
    IDataRepository Data { get; }
    IJunctionRepository Junction { get; }
    ISchemaRepository Schema { get; }
    ITemplateRepository Template { get; }

    Task Commit(bool useClearTracking = false);
    T GetRepository<T>() where T : IRepository;
}

using Noxy.NET.CaseManagement.Application.Interfaces.Repositories;

namespace Noxy.NET.CaseManagement.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAssociationRepository Association { get; }
    IAuthenticationRepository Authentication { get; }
    IDataRepository Data { get; }
    IJunctionRepository Junction { get; }
    ISchemaRepository Schema { get; }
    ITemplateRepository Template { get; }
    
    Task Commit(bool useClearTracking = false);
    T GetRepository<T>() where T : IRepository;
}

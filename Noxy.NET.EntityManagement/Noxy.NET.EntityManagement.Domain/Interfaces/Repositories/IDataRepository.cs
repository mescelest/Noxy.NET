using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameterStyle> CreateParameterStyle(EntityDataParameterStyle entity);
    Task<EntityDataParameterSystem> CreateParameterSystem(EntityDataParameterSystem entity);
    Task<EntityDataParameterText> CreateParameterText(EntityDataParameterText entity);
    Task<Guid> RemoveParameterByID(Guid id);

    Task<Dictionary<string, EntityDataParameter.Discriminator>> GetCurrentParameterByIdentifierList(IEnumerable<string> identifiers);
    Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier);
    Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier);
    Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers);
}

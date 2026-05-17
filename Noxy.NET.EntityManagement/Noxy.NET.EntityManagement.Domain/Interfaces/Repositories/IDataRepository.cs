using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameterStyle> CreateParameterStyle(Guid schemaID, string identifier, string value, DateTime? timeEffective = null);
    Task<EntityDataParameterSystem> CreateParameterSystem(Guid schemaID, string identifier, string value, DateTime? timeEffective = null);
    Task<EntityDataParameterText> CreateParameterText(Guid schemaID, string identifier, string culture, string value, DateTime? timeEffective = null);
    Task<Guid> RemoveParameterByID(Guid id);

    Task<Dictionary<string, EntityDataParameter.Discriminator>> GetCurrentParameterByIdentifierList(IEnumerable<string> identifiers);
    Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier);
    Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier);
    Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers);
}

using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameterStyle> CreateStyleParameter(Guid schemaID, string identifier, string value, DateTime? timeEffective = null);
    Task<EntityDataParameterSystem> CreateSystemParameter(Guid schemaID, string identifier, string value, DateTime? timeEffective = null);
    Task<EntityDataParameterText> CreateTextParameter(Guid schemaID, string identifier, string culture, string value, DateTime? timeEffective = null);
    Task<Guid> RemoveParameterByID(Guid id);
    Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier);
    Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier);
    Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers);
}

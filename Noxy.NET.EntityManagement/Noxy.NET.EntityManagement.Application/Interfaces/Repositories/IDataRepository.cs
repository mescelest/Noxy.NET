using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameterStyle> CreateStyleParameter(string identifier, string value, DateTime? timeEffective = null);
    Task<EntityDataParameterSystem> CreateSystemParameter(string identifier, string value, DateTime? timeEffective = null);
    Task<EntityDataParameterText> CreateTextParameter(string identifier, string value, DateTime? timeEffective = null);
    Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier);
    Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier);
    Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers);
}

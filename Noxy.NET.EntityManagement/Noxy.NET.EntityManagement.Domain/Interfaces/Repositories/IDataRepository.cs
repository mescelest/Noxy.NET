using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameter.Discriminator> GetParameterByID(Guid id);
    Task<EntityDataParameterStyle> GetParameterStyleByID(Guid id);
    Task<EntityDataParameterSystem> GetParameterSystemByID(Guid id);
    Task<EntityDataParameterText> GetParameterTextByID(Guid id);
    Task<EntityDataParameterStyle> CreateParameterStyle(EntityDataParameterStyle entity);
    Task<EntityDataParameterSystem> CreateParameterSystem(EntityDataParameterSystem entity);
    Task<EntityDataParameterText> CreateParameterText(EntityDataParameterText entity);
    void UpdateParameter(EntityDataParameter entity);
    void UpdateParameterStyle(EntityDataParameterStyle entity);
    void UpdateParameterSystem(EntityDataParameterSystem entity);
    void UpdateParameterText(EntityDataParameterText entity);
    void RemoveParameter(EntityDataParameter entity);

    Task<Dictionary<string, EntityDataParameter.Discriminator>> GetEffectiveParameterByIdentifier(string identifier);
    Task<Dictionary<string, EntityDataParameter.Discriminator>> GetCurrentParameterByIdentifierList(IEnumerable<string> identifiers);
    Task<List<EntityDataParameter.Discriminator>> GetParameterListWithIdentifier(string identifier);
    Task<EntityDataParameterText?> GetCurrentTextParameterByIdentifier(string identifier);
    Task<Dictionary<string, EntityDataParameterText?>> GetCurrentTextParameterByIdentifierList(IEnumerable<string> identifiers);
}

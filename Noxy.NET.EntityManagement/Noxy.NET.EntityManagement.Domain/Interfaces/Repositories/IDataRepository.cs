using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameter> GetParameterByID(Guid id);
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

    Task<List<EntityDataParameter>> GetParameterList();
    Task<EntityDataParameter?> GetEffectiveParameterByIdentifier(string identifier);
    Task<List<EntityDataParameter>> GetEffectiveParameterListByIdentifierList(IEnumerable<string> list);
    Task<List<EntityDataParameter>> GetParameterListByIdentifier(string identifier);
    Task<EntityDataParameterText?> GetEffectiveParameterTextByIdentifier(string identifier);
    Task<List<EntityDataParameterText>> GetEffectiveParameterTextListByIdentifierList(IEnumerable<string> identifiers);
}

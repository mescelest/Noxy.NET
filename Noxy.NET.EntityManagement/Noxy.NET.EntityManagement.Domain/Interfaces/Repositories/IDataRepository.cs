using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Models.Filters.Data;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface IDataRepository
{
    Task<List<EntityDataParameter>> GetParameterList();
    Task<EntityDataParameter> GetParameterByID(Guid id);

    Task<List<EntityDataParameter>> GetParameterListByIdentifier(string identifier, FilterDataParameterList filter);
    Task<int> GetParameterCountByIdentifier(string identifier, FilterDataParameterCount filter);

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
}

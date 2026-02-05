using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface IDataRepository
{
    Task<EntityDataParameterText> GetCurrentTextParameterByIdentifier(string identifier);
}

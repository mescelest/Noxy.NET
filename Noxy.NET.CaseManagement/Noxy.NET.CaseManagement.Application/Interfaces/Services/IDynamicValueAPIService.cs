using Noxy.NET.CaseManagement.Domain.Entities.Data;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface IDynamicValueAPIService
{
    public Task<EntityDataElement> CreateElement(string identifier, Dictionary<string, object?> data);
    public Task UpdateElement(Guid id, Dictionary<string, object?> data);
}

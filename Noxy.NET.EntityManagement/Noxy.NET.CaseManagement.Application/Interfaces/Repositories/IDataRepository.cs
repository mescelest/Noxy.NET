using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Repositories;

public interface IDataRepository
{
    Task<List<EntityDataElement>> GetElementListWithIdentifier(string identifier);
    Task<List<EntityDataProperty.Discriminator>> GetPropertyListWithIdentifierAndElementID(string identifier, Guid idElement);
    Task<List<EntityDataSystemParameter>> GetCurrentSystemParameterList();
    Task<List<EntityDataTextParameter>> GetCurrentTextParameterList();
    Task<EntityDataTextParameter> GetCurrentTextParameterByIdentifier(string identifier);

    Task<EntityDataElement> CreateElement(string identifier);
    Task UpdateElement(Guid id);
    
    Task<EntityDataProperty.Discriminator> CreateProperty(Guid element, string identifier, object? value);
}

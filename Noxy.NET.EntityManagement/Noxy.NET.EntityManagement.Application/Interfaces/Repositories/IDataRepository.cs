using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface IDataRepository
{
    Task<List<EntityDataElement>> GetElementListWithIdentifier(string identifier);
    Task<List<EntityDataProperty.Discriminator>> GetPropertyListWithIdentifierAndElementID(string identifier, Guid idElement);
    Task<EntityDataParameterText> GetCurrentTextParameterByIdentifier(string identifier);
    Task<EntityDataParameter.Discriminator[]> GetParameterList(string? search, bool isSystemDefined, bool isApprovalRequired);

    Task<EntityDataElement> CreateElement(string identifier);
    Task UpdateElement(Guid id);

    Task<EntityDataProperty.Discriminator> CreateProperty(Guid element, string identifier, object? value);
}

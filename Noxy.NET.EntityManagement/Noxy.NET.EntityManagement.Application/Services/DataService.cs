using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.Application.Services;

public class DataService(IUnitOfWorkFactory serviceUoWFactory) : IDataService
{
    public async Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        Dictionary<string, string> result = [];
        foreach (string identifier in list)
        {
            try
            {
                EntityDataParameterText entityParameterText = await uow.Data.GetCurrentTextParameterByIdentifier(identifier);
                result.Add(identifier, entityParameterText.Value);
            }
            catch
            {
                result.Add(identifier, "[MISSING]");
            }
        }

        return result;
    }
}

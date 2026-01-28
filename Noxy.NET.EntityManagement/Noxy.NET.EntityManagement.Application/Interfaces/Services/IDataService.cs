using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.ViewModels;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IDataService
{
    ViewModelSchemaContext[] GetContextList();
    ViewModelSchemaContext GetContextListWithIdentifier(string identifier);

    Task<ViewModelDataElement[]> GetElementListWithContextIdentifier(string identifier);

    Task<List<EntityDataSystemParameter>> GetSystemParameterList();
    Task<List<EntityDataTextParameter>> GetTextParameterList();

    Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list);
}

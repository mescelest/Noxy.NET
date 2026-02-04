using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Data;
using Noxy.NET.EntityManagement.Domain.ViewModels;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IDataService
{
    ViewModelSchemaContext[] GetContextList();
    ViewModelSchemaContext GetContextListWithIdentifier(string identifier);

    Task<ViewModelDataElement[]> GetElementListWithContextIdentifier(string identifier);

    Task<List<EntityDataSystemParameter>> GetSystemParameterList();
    Task<List<EntityDataTextParameter>> GetTextParameterList();

    Task<ViewModelParameter[]> GetParameterList(DataParameterListFormModel model);
    Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list);
}

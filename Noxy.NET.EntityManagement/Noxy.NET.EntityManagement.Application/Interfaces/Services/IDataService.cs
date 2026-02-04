namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IDataService
{
    Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list);
}

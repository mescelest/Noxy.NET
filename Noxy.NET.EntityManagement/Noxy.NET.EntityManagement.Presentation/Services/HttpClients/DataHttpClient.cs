using Noxy.NET.EntityManagement.Domain.ViewModels;
using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services.HttpClients;

public class DataHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseHttpClientForm(http, serviceAuthentication)
{
    public async Task<ViewModelSchemaContext[]> GetContextList()
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, "Data/Context");
        return await SendRequest<ViewModelSchemaContext[]>(request);
    }

    public async Task<ViewModelSchemaContext> GetContextWithIdentifier(string identifier)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, $"Data/Context/{identifier}");
        return await SendRequest<ViewModelSchemaContext>(request);
    }

    public async Task<ViewModelDataElement[]> GetElementListByContext(string identifier)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, $"Data/Context/{identifier}/Element");
        return await SendRequest<ViewModelDataElement[]>(request);
    }

    public async Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, "Data/TextParameter/Resolve");
        return await SendRequest<Dictionary<string, string>>(request);
    }
}

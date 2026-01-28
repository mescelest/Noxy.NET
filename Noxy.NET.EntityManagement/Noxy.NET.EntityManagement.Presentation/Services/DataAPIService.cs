using System.Net.Http.Json;
using Noxy.NET.EntityManagement.Domain.ViewModels;
using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class DataAPIService(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseServiceAPI(http, serviceAuthentication)
{
    public async Task<ViewModelSchemaContext[]> GetContextList()
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Get, $"Data/Context");
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<ViewModelSchemaContext[]>() ?? throw new FormatException();
    }

    public async Task<ViewModelSchemaContext> GetContextWithIdentifier(string identifier)
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Get, $"Data/Context/{identifier}");
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<ViewModelSchemaContext>() ?? throw new FormatException();
    }

    public async Task<ViewModelDataElement[]> GetElementListByContext(string identifier)
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Get, $"Data/Context/{identifier}/Element");
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<ViewModelDataElement[]>() ?? throw new FormatException();
    }

    public async Task<Dictionary<string, string>> ResolveTextParameterList(IEnumerable<string> list)
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Post, $"Data/TextParameter/Resolve", list);
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<Dictionary<string, string>>() ?? throw new FormatException();
    }
}

using System.Net.Http.Json;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class SchemaAPIService(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseServiceAPI(http, serviceAuthentication)
{
    public async Task<EntitySchema> GetSchemaWithID(Guid id)
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Get, $"Template/Schema/{id}");
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<EntitySchema>() ?? throw new FormatException();
    }

    public async Task<List<EntitySchema>> GetSchemaList()
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Get, "Template/Schema");
        HttpContent response = HandleResponse(await SendRequest(requestMessage));
        return await response.ReadFromJsonAsync<List<EntitySchema>>() ?? throw new FormatException();
    }

    public async Task ActivateSchema(Guid id)
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Post, $"Template/Schema/{id}/Activate");
        HandleResponse(await SendRequest(requestMessage));
    }
}

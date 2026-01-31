using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services.HttpClients;

public class SchemaHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseHttpClientForm(http, serviceAuthentication)
{
    public async Task<EntitySchema> GetSchemaWithID(Guid id)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, $"Template/Schema/{id}");
        return await SendRequest<EntitySchema>(request);
    }

    public async Task<EntitySchema[]> GetSchemaList()
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, "Template/Schema");
        return await SendRequest<EntitySchema[]>(request);
    }

    public async Task ActivateSchema(Guid id)
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Get, $"Template/Schema/{id}/Activate");
        HttpResponseMessage response = await SendRequest(request);
        if (!response.IsSuccessStatusCode) throw new("Failed to activate schema");
    }
}

using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class APIHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseHttpClientForm(http, serviceAuthentication)
{
    public async Task ActivateSchema(Guid id)
    {
        HttpResponseMessage response = await SendRequest(HttpMethod.Get, $"Template/Schema/{id}/Activate");
        if (!response.IsSuccessStatusCode) throw new("Failed to activate schema");
    }
}

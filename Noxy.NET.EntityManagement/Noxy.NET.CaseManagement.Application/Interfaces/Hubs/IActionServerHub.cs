using Noxy.NET.CaseManagement.Domain.Models;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Hubs;

public interface IActionServerHub
{
    StateAction Register(Guid id, string identifier, Dictionary<string, object?>? context = null);
    void Deregister(Guid id);
    StateAction CommitField(Guid id, string identifier, JsonProperty value);
    object? Submit(Guid id);
}

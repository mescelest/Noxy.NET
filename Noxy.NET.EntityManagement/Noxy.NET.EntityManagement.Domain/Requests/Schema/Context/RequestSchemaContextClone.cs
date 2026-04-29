using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;

public class RequestSchemaContextClone : BaseRequestPost<ResponseSchemaContextClone>
{
    public override string APIEndpoint => $"Schema/Context/{ID}/Clone";

    [Required]
    public required Guid ID { get; init; }
}

using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaContextDelete : BaseRequestPost<ResponseSchemaContextDelete>
{
    public override string APIEndpoint => $"Schema/Context/{ID}/Delete";

    [Required]
    public required Guid ID { get; init; }
}

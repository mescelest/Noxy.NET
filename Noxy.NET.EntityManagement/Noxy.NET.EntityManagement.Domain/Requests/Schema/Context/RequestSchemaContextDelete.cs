using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;

public class RequestSchemaContextDelete : BaseRequestPost<ResponseSchemaContextDelete>
{
    public override string APIEndpoint => $"schema/context/{ID}/delete";

    [Required]
    public required Guid ID { get; init; }
}

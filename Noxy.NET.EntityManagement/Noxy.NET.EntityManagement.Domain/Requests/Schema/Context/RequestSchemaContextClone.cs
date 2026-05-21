using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;

public class RequestSchemaContextClone : BaseRequestPost<ResponseSchemaContextClone>
{
    public override string APIEndpoint => $"schema/context/{ID}/clone";

    [Required]
    public required Guid ID { get; init; }
}

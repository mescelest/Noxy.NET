using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaActivate : BaseRequestPost<ResponseSchemaActivate>
{
    public override string APIEndpoint => $"schema/{ID}/activate";

    [Required]
    public required Guid ID { get; init; }
}

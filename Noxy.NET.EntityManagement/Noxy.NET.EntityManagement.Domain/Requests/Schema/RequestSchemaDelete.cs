using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaDelete : BaseRequestPost<ResponseSchemaDelete>
{
    public override string APIEndpoint => $"schema/{ID}/delete";

    [Required]
    public required Guid ID { get; init; }
}

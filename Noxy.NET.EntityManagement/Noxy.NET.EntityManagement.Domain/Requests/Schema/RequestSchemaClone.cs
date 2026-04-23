using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaClone : BaseRequestPost<ResponseSchemaClone>
{
    public override string APIEndpoint => $"Schema/{ID}/Clone";

    [Required]
    public required Guid ID { get; init; }
}

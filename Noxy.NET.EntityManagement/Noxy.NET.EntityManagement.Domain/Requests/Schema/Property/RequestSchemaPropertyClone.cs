using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyClone : BaseRequestPost<ResponseSchemaPropertyClone>
{
    public override string APIEndpoint => $"Schema/Element/{ID}/Clone";

    [Required]
    public required Guid ID { get; init; }
}

using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyDelete : BaseRequestPost<ResponseSchemaPropertyDelete>
{
    public override string APIEndpoint => $"schema/property/{ID}/delete";

    [Required]
    public required Guid ID { get; init; }
}

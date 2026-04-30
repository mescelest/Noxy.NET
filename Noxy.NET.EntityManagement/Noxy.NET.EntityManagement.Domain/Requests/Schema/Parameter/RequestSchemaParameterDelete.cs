using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;

public class RequestSchemaParameterDelete : BaseRequestPost<ResponseSchemaParameterDelete>
{
    public override string APIEndpoint => $"Schema/Element/{ID}/Delete";

    [Required]
    public required Guid ID { get; init; }
}

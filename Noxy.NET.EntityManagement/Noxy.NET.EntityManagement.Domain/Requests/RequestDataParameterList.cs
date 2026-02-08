using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataParameterList : BaseRequestGet<ResponseDataParameterList>
{
    public override string APIEndpoint => $"/Data/Parameter/Text/{SchemaIdentifier}";

    [Required]
    public required string SchemaIdentifier { get; init; }
}

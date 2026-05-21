using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterStyleResolve : BaseRequestGet<ResponseDataParameterStyleResolve>
{
    public override string APIEndpoint => $"/data/parameter/style/{SchemaIdentifier}/resolve";

    [Required]
    public required string SchemaIdentifier { get; init; }
}

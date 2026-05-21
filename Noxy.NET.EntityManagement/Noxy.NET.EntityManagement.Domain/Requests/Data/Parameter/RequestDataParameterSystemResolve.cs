using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterSystemResolve : BaseRequestGet<ResponseDataParameterSystemResolve>
{
    public override string APIEndpoint => $"/data/parameter/system/{SchemaIdentifier}/resolve";

    [Required]
    public required string SchemaIdentifier { get; init; }
}

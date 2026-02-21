using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data;

public class RequestDataParameterTextResolve : BaseRequestGet<ResponseDataParameterTextResolve>
{
    public override string APIEndpoint => $"/Data/Parameter/Text/{SchemaIdentifier}/Resolve";

    [Required]
    public required string SchemaIdentifier { get; init; }
}

using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterTextResolve : BaseRequestGet<ResponseDataParameterTextResolve>
{
    public override string APIEndpoint => $"/data/parameter/text/{SchemaIdentifier}/resolve";

    [Required]
    public required string SchemaIdentifier { get; init; }
}

using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataParameterTextResolve : BaseRequestGet<ResponseDataParameterResolve>
{
    public override string APIEndpoint => $"/Data/Parameter/Text/{SchemaIdentifier}/Resolve";

    [Required]
    public required string SchemaIdentifier { get; set; }
}

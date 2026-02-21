using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data;

public class RequestDataParameterTextResolveList : BaseRequestPost<ResponseDataParameterResolveList>
{
    public override string APIEndpoint => "/Data/Parameter/Text/Resolve";

    [Required]
    public required IEnumerable<string> SchemaIdentifierList { get; init; }
}

using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterTextResolveList : BaseRequestPost<ResponseDataParameterTextResolveList>
{
    public override string APIEndpoint => "/data/parameter/text/resolve";

    [Required]
    public required IEnumerable<string> SchemaIdentifierList { get; init; }
}

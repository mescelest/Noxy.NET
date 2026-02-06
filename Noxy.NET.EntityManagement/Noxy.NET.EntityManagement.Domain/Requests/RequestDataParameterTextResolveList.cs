using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataParameterTextResolveList : BaseRequestPost<Dictionary<string, string>>
{
    public override string APIEndpoint => "/Data/Parameter/Text/Resolve";

    [Required]
    public required IEnumerable<string> SchemaIdentifierList { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataCreateSystemParameter : BaseRequestGet<ResponseDataCreateSystemParameter>
{
    public override string APIEndpoint => "/Data/Parameter/System";

    [Required]
    public required string SchemaIdentifier { get; set; }

    [Required]
    public string Value { get; set; } = string.Empty;

    [Required]
    public DateTime? DateEffective { get; set; }
}

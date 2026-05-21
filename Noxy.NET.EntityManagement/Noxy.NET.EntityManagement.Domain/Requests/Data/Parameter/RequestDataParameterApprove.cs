using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterApprove : BaseRequestPost<ResponseDataParameterApprove>
{
    public override string APIEndpoint => $"/data/parameter/{ID}/approve";

    [Required]
    public required Guid ID { get; init; }
}

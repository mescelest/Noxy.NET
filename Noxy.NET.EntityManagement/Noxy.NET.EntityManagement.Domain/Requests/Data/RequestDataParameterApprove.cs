using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data;

public class RequestDataParameterApprove : BaseRequestPost<ResponseDataParameterApprove>
{
    public override string APIEndpoint => $"/Data/Parameter/{ID}/Approve";

    [Required]
    public required Guid ID { get; init; }
}

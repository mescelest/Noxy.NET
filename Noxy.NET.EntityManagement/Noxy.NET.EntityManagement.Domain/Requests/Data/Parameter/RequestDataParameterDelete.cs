using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterDelete : BaseRequestPost<ResponseDataParameterDelete>
{
    public override string APIEndpoint => $"/data/parameter/{ID}/delete";

    [Required]
    public required Guid ID { get; init; }
}

using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterFind : BaseRequestGet<ResponseDataParameterFind>
{
    public override string APIEndpoint => $"/data/parameter/{ID}";

    public required Guid ID { get; set; }
}

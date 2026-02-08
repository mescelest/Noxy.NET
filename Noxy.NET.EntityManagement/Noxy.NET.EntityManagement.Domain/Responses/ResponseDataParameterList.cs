using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses;

public class ResponseDataParameterList
{
    public required List<EntityDataParameter.Discriminator> Value { get; set; }
}

using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

public class ResponseDataParameterFind(EntityDataParameter.Discriminator value)
{
    public EntityDataParameter.Discriminator Value { get; } = value;
}

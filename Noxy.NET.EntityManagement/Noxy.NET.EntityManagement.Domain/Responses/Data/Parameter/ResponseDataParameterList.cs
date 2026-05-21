using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

public class ResponseDataParameterList(List<EntityDataParameter.Discriminator> value)
{
    public List<EntityDataParameter.Discriminator> Value { get; } = value;
}

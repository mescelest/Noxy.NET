using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data;

public class EntityDataParameterText : EntityDataParameter
{
    public string Culture { get; set; } = "en";
}

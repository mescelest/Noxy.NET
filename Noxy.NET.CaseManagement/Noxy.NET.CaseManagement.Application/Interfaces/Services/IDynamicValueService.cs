using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface IDynamicValueService
{
    public void Initialize(EntitySchema schema, List<EntityDataSystemParameter> listSystemParameter, List<EntityDataTextParameter> listTextParameter);

    object? Resolve(EntitySchemaDynamicValue? value, object? data = null, object? context = null);
    object? Resolve(EntitySchemaDynamicValue.Discriminator? value, object? data = null, object? context = null);
    string? ResolveAsString(EntitySchemaDynamicValue? value, object? data = null, object? context = null);
    string? ResolveAsString(EntitySchemaDynamicValue.Discriminator? value, object? data = null, object? context = null);
    
    object? Execute(string identifier, object? data = null, object? context = null);
}

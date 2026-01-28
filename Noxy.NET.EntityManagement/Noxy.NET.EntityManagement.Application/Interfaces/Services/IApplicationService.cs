using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IApplicationService
{
    void SetSchema(EntitySchema schema);

    EntitySchema GetSchema();

    List<EntitySchemaContext> GetSchemaContext();
    EntitySchemaContext GetSchemaContext(string identifier);

    EntitySchemaElement GetSchemaElement(string identifier);

    EntitySchemaProperty.Discriminator GetSchemaProperty(string identifier);
}

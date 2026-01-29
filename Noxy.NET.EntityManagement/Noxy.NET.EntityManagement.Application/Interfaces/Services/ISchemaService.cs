using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface ISchemaService
{
    Task<EntitySchemaContext> CreateOrUpdate(FormModelSchemaContext model);
    Task<EntitySchemaParameter.Discriminator> CreateOrUpdate(FormModelSchemaParameterStyle model);
    Task<EntitySchemaParameter.Discriminator> CreateOrUpdate(FormModelSchemaParameterSystem model);
    Task<EntitySchemaParameter.Discriminator> CreateOrUpdate(FormModelSchemaParameterText model);
    Task<EntitySchemaElement> CreateOrUpdate(FormModelSchemaElement model);
    Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyBoolean model);
    Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyDateTime model);
    Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyDecimal model);
    Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyInteger model);
    Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyImage model);
    Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyString model);
}

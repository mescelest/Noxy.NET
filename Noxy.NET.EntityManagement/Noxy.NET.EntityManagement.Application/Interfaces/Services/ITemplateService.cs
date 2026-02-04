using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface ITemplateService
{
    Task<List<EntitySchema>> GetSchemaList();
    Task<EntitySchema> GetSchemaWithID(Guid id);
    Task<EntitySchema> GetCurrentSchema();

    Task ActivateSchema(Guid id);

    Task<EntitySchema> CreateOrUpdate(FormModelSchema model);
}

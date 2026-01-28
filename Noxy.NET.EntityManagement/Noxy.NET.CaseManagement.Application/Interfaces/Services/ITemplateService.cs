using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface ITemplateService
{
    Task<List<EntitySchema>> GetSchemaList();
    Task<EntitySchema> GetSchemaWithID(Guid id);
    Task<EntitySchema> GetCurrentSchema();
    
    Task ActivateSchema(Guid id);
    
    Task<EntitySchema> CreateOrUpdate(FormModelSchema model);
}

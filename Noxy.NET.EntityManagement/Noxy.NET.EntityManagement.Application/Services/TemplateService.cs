using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

namespace Noxy.NET.EntityManagement.Application.Services;

public class TemplateService(IUnitOfWorkFactory serviceUoWFactory, ISchemaBuilderService serviceSchemaBuilder, IApplicationService serviceApplication, IDataService serviceData) : ITemplateService
{
    public async Task<List<EntitySchema>> GetSchemaList()
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        List<EntitySchema> listSchema = await uow.Template.GetSchemaList();
        return listSchema;
    }

    public async Task<EntitySchema> GetSchemaWithID(Guid id)
    {
        return await serviceSchemaBuilder.ConstructSchema(id);
    }

    public async Task<EntitySchema> GetCurrentSchema()
    {
        return await serviceSchemaBuilder.ConstructSchema();
    }

    public async Task<EntitySchema> CreateOrUpdate(FormModelSchema model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchema result;
        if (model.ID == Guid.Empty)
        {
            result = await uow.Template.Create(new()
            {
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                IsActive = false,
                TimeActivated = null
            });
        }
        else
        {
            result = await uow.Template.GetSchemaByID(model.ID);

            result.Name = model.Name;
            result.Note = model.Note;

            uow.Template.Update(result);
            await uow.Commit();
        }

        await uow.Commit();
        return result;
    }

    public async Task ActivateSchema(Guid id)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchema entitySchema = await GetSchemaWithID(id);
        entitySchema.IsActive = true;
        entitySchema.TimeActivated = DateTime.UtcNow;
        uow.Template.Update(entitySchema);

        List<EntitySchema> listSchema = await uow.Template.GetSchemaList();
        foreach (EntitySchema schema in listSchema.Where(schema => entitySchema.ID != schema.ID))
        {
            schema.IsActive = false;
            uow.Template.Update(schema);
        }

        await uow.Commit();

        serviceApplication.SetSchema(entitySchema);
    }
}

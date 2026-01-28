using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

namespace Noxy.NET.EntityManagement.Application.Services;

public class SchemaService(IUnitOfWorkFactory serviceUoWFactory) : ISchemaService
{
    public async Task<EntitySchemaContext> CreateOrUpdate(FormModelSchemaContext model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaContext result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaContext
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaContextByID(model.ID), model);
            uow.Schema.Update(result);
        }

        if (model.ElementList != null)
        {
            await uow.Junction.ClearSchemaContextHasElementByEntityID(result.ID);

            foreach (FormModelSchemaContext.HasElement item in model.ElementList)
            {
                EntityJunctionSchemaContextHasElement parsed = await uow.Junction.Create(new EntityJunctionSchemaContextHasElement()
                {
                    ID = item.ID != Guid.Empty ? item.ID : BaseEntity.CreateID(),
                    Order = item.Order,
                    EntityID = result.ID,
                    RelationID = item.RelationID
                });
                result.ElementList?.Add(parsed);
            }
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaDynamicValue.Discriminator> CreateOrUpdate(FormModelSchemaDynamicValueStyleParameter model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaDynamicValue.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaDynamicValueStyleParameter
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                IsApprovalRequired = model.IsApprovalRequired,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaDynamicValueByID(model.ID);
            EntitySchemaDynamicValueSystemParameter property = UpdateSchemaFields(result.SystemParameter, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaDynamicValue.Discriminator> CreateOrUpdate(FormModelSchemaDynamicValueSystemParameter model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaDynamicValue.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaDynamicValueSystemParameter
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                IsApprovalRequired = model.IsApprovalRequired,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaDynamicValueByID(model.ID);
            EntitySchemaDynamicValueSystemParameter property = UpdateSchemaFields(result.SystemParameter, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaDynamicValue.Discriminator> CreateOrUpdate(FormModelSchemaDynamicValueTextParameter model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaDynamicValue.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaDynamicValueTextParameter
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                Type = model.Type,
                IsApprovalRequired = model.IsApprovalRequired,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaDynamicValueByID(model.ID);
            EntitySchemaDynamicValueTextParameter property = UpdateSchemaFields(result.TextParameter, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaElement> CreateOrUpdate(FormModelSchemaElement model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaElement result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaElement
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaElementByID(model.ID), model);
            uow.Schema.Update(result);
        }

        if (model.PropertyList != null)
        {
            await uow.Junction.ClearSchemaElementHasPropertyByEntityID(result.ID);

            foreach (FormModelSchemaElement.HasProperty item in model.PropertyList)
            {
                EntityJunctionSchemaElementHasProperty parsed = await uow.Junction.Create(new EntityJunctionSchemaElementHasProperty()
                {
                    ID = item.ID != Guid.Empty ? item.ID : BaseEntity.CreateID(),
                    Order = item.Order,
                    EntityID = result.ID,
                    RelationID = item.RelationID
                });
                result.PropertyList?.Add(parsed);
            }
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyBoolean model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaProperty.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaPropertyBoolean
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyBoolean property = UpdateSchemaFields(result.Boolean, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyDateTime model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaProperty.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaPropertyDateTime
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                Type = model.Type,
                SchemaID = model.SchemaID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyDateTime property = UpdateSchemaFields(result.DateTime, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyDecimal model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaProperty.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaPropertyDecimal
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                SchemaID = model.SchemaID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyDecimal property = UpdateSchemaFields(result.Decimal, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyInteger model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaProperty.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaPropertyInteger
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                IsUnsigned = model.IsUnsigned,
                SchemaID = model.SchemaID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyInteger property = UpdateSchemaFields(result.Integer, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyString model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaProperty.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaPropertyString
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                SchemaID = model.SchemaID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyString property = UpdateSchemaFields(result.String, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaProperty.Discriminator> CreateOrUpdate(FormModelSchemaPropertyImage model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaProperty.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaPropertyImage
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                AllowedExtensions = model.AllowedExtensions,
                SchemaID = model.SchemaID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyInteger property = UpdateSchemaFields(result.Integer, model);
            uow.Schema.Update(property);
        }

        await uow.Commit();
        return result;
    }

    #region -- Private methods --

    private static TEntity UpdateSchemaFields<TEntity>(TEntity? entity, BaseFormModelEntitySchema model) where TEntity : BaseEntitySchema
    {
        if (entity == null) throw new NullReferenceException();

        entity.Name = model.Name;
        entity.Note = model.Note;
        entity.SchemaIdentifier = model.SchemaIdentifier;

        return entity;
    }

    private static TEntity UpdateSchemaFields<TEntity>(TEntity? entity, BaseFormModelEntitySchemaComponent model) where TEntity : BaseEntitySchemaComponent
    {
        if (entity == null) throw new NullReferenceException();

        return UpdateSchemaFields(entity, model as BaseFormModelEntitySchema);
    }

    #endregion
}

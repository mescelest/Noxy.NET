using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.AssociationForms;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Application.Services;

public class SchemaService(IUnitOfWorkFactory serviceUoWFactory) : ISchemaService
{
    public async Task<EntitySchemaAction> CreateOrUpdate(FormModelSchemaAction model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaAction result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaAction
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaActionByID(model.ID), model);
            uow.Schema.Update(result);
        }

        if (model.ActionStepList != null)
        {
            await uow.Junction.ClearSchemaActionHasActionStepByEntityID(result.ID);

            foreach (FormModelSchemaAction.HasActionStep item in model.ActionStepList)
            {
                EntityJunctionSchemaActionHasActionStep parsed = await uow.Junction.Create(new EntityJunctionSchemaActionHasActionStep
                {
                    ID = item.ID != Guid.Empty ? item.ID : BaseEntity.CreateID(),
                    Order = item.Order,
                    EntityID = result.ID,
                    RelationID = item.RelationID
                });
                result.ActionStepList?.Add(parsed);
            }
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaActionInput> CreateOrUpdate(FormModelSchemaActionInput model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaActionInput result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaActionInput
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                SchemaID = model.SchemaID,
                InputID = model.InputID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaActionInputByID(model.ID), model);
            result.InputID = model.InputID;
            uow.Schema.Update(result);
        }

        if (model.AttributeList != null)
        {
            result.AttributeList = [];
            await uow.Association.ClearActionInputAttribute(model.ID);

            foreach (FormModelAssociationSchemaActionInputHasAttribute.Discriminator discriminator in model.AttributeList)
            {
                FormModelAssociationSchemaActionInputHasAttribute item = discriminator.GetValue();
                List<EntityAssociationSchemaActionInputHasAttribute> parsed = item switch
                {
                    FormModelAssociationSchemaActionInputHasAttribute<string> value => await uow.Association.AssociateActionInputWithAttribute(result.ID, value.RelationID, value.Value),
                    FormModelAssociationSchemaActionInputHasAttribute<int?> value => await uow.Association.AssociateActionInputWithAttribute(result.ID, value.RelationID, value.Value),
                    FormModelAssociationSchemaActionInputHasAttribute<GenericUUID<EntitySchemaDynamicValue>?> value => await uow.Association.AssociateActionInputWithAttribute(result.ID, value.RelationID, value.Value),
                    _ => throw new ArgumentOutOfRangeException(nameof(model))
                };

                result.AttributeList?.AddRange(parsed.Select(x => new EntityAssociationSchemaActionInputHasAttribute.Discriminator(x)));
            }
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaActionStep> CreateOrUpdate(FormModelSchemaActionStep model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaActionStep result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaActionStep
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                IsRepeatable = model.IsRepeatable,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaActionStepByID(model.ID), model);
            uow.Schema.Update(result);
        }

        if (model.ActionInputList != null)
        {
            await uow.Junction.ClearSchemaActionStepHasActionInputByEntityID(result.ID);

            foreach (FormModelSchemaActionStep.HasActionInput item in model.ActionInputList)
            {
                EntityJunctionSchemaActionStepHasActionInput parsed = await uow.Junction.Create(new EntityJunctionSchemaActionStepHasActionInput
                {
                    ID = item.ID != Guid.Empty ? item.ID : BaseEntity.CreateID(),
                    Order = item.Order,
                    EntityID = result.ID,
                    RelationID = item.RelationID
                });
                result.ActionInputList?.Add(parsed);
            }
        }

        await uow.Commit();
        return result;
    }

    public async Task<EntitySchemaAttribute> CreateOrUpdate(FormModelSchemaAttribute model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaAttribute result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaAttribute
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                Type = model.Type,
                IsValueList = model.IsValueList,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaAttributeByID(model.ID), model);
            uow.Schema.Update(result);
        }

        await uow.Commit();
        return result;
    }

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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = UpdateSchemaFields(await uow.Schema.GetSchemaContextByID(model.ID), model);
            uow.Schema.Update(result);
        }

        if (model.ActionList != null)
        {
            await uow.Junction.ClearSchemaContextHasActionByEntityID(result.ID);

            foreach (FormModelSchemaContext.HasAction item in model.ActionList)
            {
                EntityJunctionSchemaContextHasAction parsed = await uow.Junction.Create(new EntityJunctionSchemaContextHasAction()
                {
                    ID = item.ID != Guid.Empty ? item.ID : BaseEntity.CreateID(),
                    Order = item.Order,
                    EntityID = result.ID,
                    RelationID = item.RelationID
                });
                result.ActionList?.Add(parsed);
            }
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

    public async Task<EntitySchemaDynamicValue.Discriminator> CreateOrUpdate(FormModelSchemaDynamicValueCode model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaDynamicValue.Discriminator result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaDynamicValueCode
            {
                SchemaIdentifier = model.SchemaIdentifier,
                Name = model.Name,
                Note = model.Note,
                Order = model.Order,
                Value = model.CodeSnippet,
                IsAsynchronous = model.IsAsynchronous,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaDynamicValueByID(model.ID);
            EntitySchemaDynamicValueCode property = UpdateSchemaFields(result.Code, model);
            uow.Schema.Update(property);
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
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

    public async Task<EntitySchemaInput> CreateOrUpdate(FormModelSchemaInput model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntitySchemaInput result;

        if (model.ID == Guid.Empty)
        {
            result = await uow.Schema.Create(new EntitySchemaInput
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
            result = UpdateSchemaFields(await uow.Schema.GetSchemaInputByID(model.ID), model);
            uow.Schema.Update(result);
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                DefaultValueDynamicID = model.DefaultValueDynamicID,
                SchemaID = model.SchemaID
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyBoolean property = UpdateSchemaFields(result.Boolean, model);
            property.DefaultValueDynamicID = model.DefaultValueDynamicID;
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                DefaultValueDynamicID = model.DefaultValueDynamicID,
                MinDynamicID = model.MinDynamicID,
                MaxDynamicID = model.MaxDynamicID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyDateTime property = UpdateSchemaFields(result.DateTime, model);
            property.DefaultValueDynamicID = model.DefaultValueDynamicID;
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                DefaultValueDynamicID = model.DefaultValueDynamicID,
                MinDynamicID = model.MinDynamicID,
                MaxDynamicID = model.MaxDynamicID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyDecimal property = UpdateSchemaFields(result.Decimal, model);
            property.DefaultValueDynamicID = model.DefaultValueDynamicID;
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                DefaultValueDynamicID = model.DefaultValueDynamicID,
                WidthDynamicID = model.WidthDynamicID,
                HeightDynamicID = model.HeightDynamicID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyInteger property = UpdateSchemaFields(result.Integer, model);
            property.DefaultValueDynamicID = model.DefaultValueDynamicID;
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                DefaultValueDynamicID = model.DefaultValueDynamicID,
                MinDynamicID = model.MinDynamicID,
                MaxDynamicID = model.MaxDynamicID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyInteger property = UpdateSchemaFields(result.Integer, model);
            property.DefaultValueDynamicID = model.DefaultValueDynamicID;
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
                TitleDynamicID = model.TitleDynamicID,
                DescriptionDynamicID = model.DescriptionDynamicID,
                DefaultValueDynamicID = model.DefaultValueDynamicID,
                MinDynamicID = model.MinDynamicID,
                MaxDynamicID = model.MaxDynamicID,
            });
        }
        else
        {
            result = await uow.Schema.GetSchemaPropertyByID(model.ID);
            EntitySchemaPropertyString property = UpdateSchemaFields(result.String, model);
            property.DefaultValueDynamicID = model.DefaultValueDynamicID;
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

        entity.TitleDynamicID = model.TitleDynamicID;
        entity.DescriptionDynamicID = model.DescriptionDynamicID;

        return UpdateSchemaFields(entity, model as BaseFormModelEntitySchema);
    }

    #endregion
}

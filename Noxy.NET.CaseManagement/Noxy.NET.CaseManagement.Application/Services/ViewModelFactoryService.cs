using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Domain.ViewModels;

namespace Noxy.NET.CaseManagement.Application.Services;

public class ViewModelFactoryService(IDynamicValueService serviceDynamicValue, IApplicationService serviceApplication) : IViewModelFactoryService
{
    public ViewModelSchemaDynamicValue Create(EntitySchemaDynamicValue.Discriminator? entity)
    {
        return Create(entity?.GetValue());
    }

    public ViewModelSchemaDynamicValue Create(EntitySchemaDynamicValue? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = new(serviceDynamicValue.Resolve(entity))
        };
    }

    public ViewModelSchemaAction Create(EntitySchemaAction? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = entity.Order,
            Title = serviceDynamicValue.ResolveAsString(entity.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(entity.DescriptionDynamic) ?? string.Empty,
            ActionStepList = entity.ActionStepList?.Select(Create).ToArray() ?? []
        };
    }

    public ViewModelSchemaActionHasActionStep Create(EntityJunctionSchemaActionHasActionStep? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            ActionStep = Create(entity.Relation)
        };
    }

    public ViewModelSchemaActionInput Create(EntitySchemaActionInput? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = entity.Order,
            Title = serviceDynamicValue.ResolveAsString(entity.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(entity.DescriptionDynamic) ?? string.Empty,
            Input = Create(entity.Input),
            ActionInputAttributeList = entity.AttributeList?.Select(x => Create(x.GetValue())).ToArray() ?? []
        };
    }

    public ViewModelSchemaActionInputHasAttribute Create(EntityAssociationSchemaActionInputHasAttribute? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new()
        {
            ID = entity.ID
        };
    }

    public ViewModelSchemaActionStep Create(EntitySchemaActionStep? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = entity.Order,
            Title = serviceDynamicValue.ResolveAsString(entity.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(entity.DescriptionDynamic) ?? string.Empty,
            ActionInputList = entity.ActionInputList?.Select(Create).ToArray() ?? []
        };
    }

    public ViewModelSchemaActionStepHasActionInput Create(EntityJunctionSchemaActionStepHasActionInput? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            ActionInput = Create(entity.Relation)
        };
    }

    public ViewModelSchemaContext Create(EntitySchemaContext? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = entity.Order,
            Title = serviceDynamicValue.ResolveAsString(entity.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(entity.DescriptionDynamic) ?? string.Empty
        };
    }

    public ViewModelSchemaInput Create(EntitySchemaInput? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier
        };
    }

    public ViewModelSchemaProperty Create(EntitySchemaProperty? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = entity.Order,
            Title = serviceDynamicValue.ResolveAsString(entity.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(entity.DescriptionDynamic) ?? string.Empty,
            DefaultValue = string.Empty,
        };
    }

    public ViewModelDataElement Create(EntityDataElement? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        EntitySchemaElement schema = serviceApplication.GetSchemaElement(entity.SchemaIdentifier);

        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = schema.Order,
            Title = serviceDynamicValue.ResolveAsString(schema.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(schema.DescriptionDynamic) ?? string.Empty,
            PropertyList = entity.PropertyList?.Select(Create).ToList() ?? []
        };
    }

    public ViewModelDataProperty Create(EntityDataProperty.Discriminator? entity)
    {
        return Create(entity?.GetValue());
    }

    public ViewModelDataProperty Create(EntityDataProperty? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        EntitySchemaProperty schema = serviceApplication.GetSchemaProperty(entity.SchemaIdentifier).GetValue();

        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Order = schema.Order,
            Title = serviceDynamicValue.ResolveAsString(schema.TitleDynamic) ?? string.Empty,
            Description = serviceDynamicValue.ResolveAsString(schema.DescriptionDynamic) ?? string.Empty,
            Value = new(entity switch
            {
                EntityDataPropertyBoolean parsed => parsed.Value,
                EntityDataPropertyDateTime parsed => parsed.Value,
                EntityDataPropertyString parsed => parsed.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(entity))
            })
        };
    }
}
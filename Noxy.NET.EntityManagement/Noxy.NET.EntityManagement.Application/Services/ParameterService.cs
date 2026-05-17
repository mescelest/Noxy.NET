using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Application.Services;

public class ParameterService : IParameterService
{
    private Dictionary<string, EntitySchemaParameter.Discriminator> ParameterMap { get; set; } = new();
    private Dictionary<EntitySchemaParameter.Discriminator, EntityDataParameter.Discriminator> ParameterCollection { get; set; } = new();

    public void SetParameterCollection(Dictionary<EntitySchemaParameter.Discriminator, EntityDataParameter.Discriminator> collection)
    {
        ParameterMap = collection.ToDictionary(x => x.Key.SchemaIdentifier, x => x.Key);
        ParameterCollection = collection;
    }

    public void SetParameter(EntitySchemaParameter.Discriminator parameterSchema, EntityDataParameter.Discriminator parameterData)
    {
        ParameterMap[parameterSchema.SchemaIdentifier] = parameterSchema;
        ParameterCollection[parameterSchema] = parameterData;
    }

    public bool TryGetParameter(string identifier, [NotNullWhen(true)] out EntityDataParameter.Discriminator? parameter)
    {
        parameter = null;
        return ParameterMap.TryGetValue(identifier, out EntitySchemaParameter.Discriminator? value) && ParameterCollection.TryGetValue(value, out parameter);
    }

    public bool TryGetParameterStyle(string identifier, [NotNullWhen(true)] out EntityDataParameterStyle? parameter) => TryGetParameterOfType(identifier, out parameter);
    public bool TryGetParameterSystem(string identifier, [NotNullWhen(true)] out EntityDataParameterSystem? parameter) => TryGetParameterOfType(identifier, out parameter);
    public bool TryGetParameterText(string identifier, [NotNullWhen(true)] out EntityDataParameterText? parameter) => TryGetParameterOfType(identifier, out parameter);

    public bool TryGetParameterStyleValue(string identifier, [NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!ParameterMap.TryGetValue(identifier, out EntitySchemaParameter.Discriminator? discriminatorSchema)) return false;
        if (!ParameterCollection.TryGetValue(discriminatorSchema, out EntityDataParameter.Discriminator? discriminatorData)) return false;
        if (discriminatorData.GetValue() is not EntityDataParameterStyle entityData) return false;

        value = entityData.Value;
        return true;
    }

    public bool TryGetParameterSystemValue<T>(string identifier, [NotNullWhen(true)] out T? value)
    {
        value = default;
        if (!ParameterMap.TryGetValue(identifier, out EntitySchemaParameter.Discriminator? discriminatorSchema)) return false;
        if (discriminatorSchema.GetValue() is not EntitySchemaParameterSystem entitySchema) return false;
        if (!ParameterCollection.TryGetValue(discriminatorSchema, out EntityDataParameter.Discriminator? discriminatorData)) return false;
        if (discriminatorData.GetValue() is not EntityDataParameterSystem entityData) return false;

        object? parsed = entitySchema.Type switch
        {
            ParameterSystemTypeEnum.Boolean => bool.Parse(entityData.Value),
            ParameterSystemTypeEnum.Integer => int.Parse(entityData.Value),
            ParameterSystemTypeEnum.Decimal => double.Parse(entityData.Value),
            ParameterSystemTypeEnum.Guid => Guid.Parse(entityData.Value),
            ParameterSystemTypeEnum.DateTime => DateTime.Parse(entityData.Value),
            ParameterSystemTypeEnum.String => entityData.Value,
            _ => null
        };

        if (parsed is not T typed) return false;

        value = typed;
        return true;
    }

    public bool TryGetParameterTextValue(string identifier, [NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!ParameterMap.TryGetValue(identifier, out EntitySchemaParameter.Discriminator? discriminatorSchema)) return false;
        if (!ParameterCollection.TryGetValue(discriminatorSchema, out EntityDataParameter.Discriminator? discriminatorData)) return false;
        if (discriminatorData.GetValue() is not EntityDataParameterText entityData) return false;

        value = entityData.Value;
        return true;
    }

    private bool TryGetParameterOfType<T>(string identifier, [NotNullWhen(true)] out T? parameter) where T : EntityDataParameter
    {
        parameter = null;
        return TryGetParameter(identifier, out EntityDataParameter.Discriminator? discriminator)
               && discriminator.GetValue() is T value
               && (parameter = value) is not null;
    }
}

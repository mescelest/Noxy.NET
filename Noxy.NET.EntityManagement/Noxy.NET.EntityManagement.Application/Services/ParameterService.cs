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
    private readonly Dictionary<string, ParameterPair> _parameters = new();

    public void SetParameterCollection(Dictionary<EntitySchemaParameter.Discriminator, EntityDataParameter.Discriminator> collection)
    {
        _parameters.Clear();
        foreach ((EntitySchemaParameter.Discriminator schema, EntityDataParameter.Discriminator data) in collection)
        {
            _parameters[schema.SchemaIdentifier] = new(schema, data);
        }
    }

    public void SetParameter(EntitySchemaParameter.Discriminator parameterSchema, EntityDataParameter.Discriminator parameterData)
    {
        _parameters[parameterSchema.SchemaIdentifier] = new(parameterSchema, parameterData);
    }

    public bool TryGetParameter(string identifier, [NotNullWhen(true)] out EntityDataParameter.Discriminator? parameter)
    {
        parameter = null;
        if (!TryResolve(identifier, out _, out EntityDataParameter.Discriminator? data)) return false;
        parameter = data;
        return true;
    }

    public bool TryGetParameterStyle(string identifier, [NotNullWhen(true)] out EntityDataParameterStyle? parameter) => TryGetTyped(identifier, out parameter);
    public bool TryGetParameterSystem(string identifier, [NotNullWhen(true)] out EntityDataParameterSystem? parameter) => TryGetTyped(identifier, out parameter);
    public bool TryGetParameterText(string identifier, [NotNullWhen(true)] out EntityDataParameterText? parameter) => TryGetTyped(identifier, out parameter);

    public bool TryGetParameterStyleValue(string identifier, [NotNullWhen(true)] out string? value)
    {
        value = null;

        if (!TryGetParameterStyle(identifier, out EntityDataParameterStyle? style))
            return false;

        value = style.Value;
        return true;
    }

    public bool TryGetParameterSystemValue<T>(string identifier, [NotNullWhen(true)] out T? value)
    {
        value = default;

        if (!TryResolve(identifier, out EntitySchemaParameter.Discriminator? schemaDisc, out EntityDataParameter.Discriminator? dataDisc))
            return false;

        EntitySchemaParameter valueSchema = schemaDisc.GetValue();
        if (valueSchema is not EntitySchemaParameterSystem schema) return false;

        EntityDataParameter valueData = dataDisc.GetValue();
        if (valueData is not EntityDataParameterSystem data) return false;

        object? parsed = schema.Type switch
        {
            ParameterSystemTypeEnum.Boolean => bool.TryParse(data.Value, out bool x) ? x : null,
            ParameterSystemTypeEnum.Integer => int.TryParse(data.Value, out int x) ? x : null,
            ParameterSystemTypeEnum.Decimal => decimal.TryParse(data.Value, out decimal x) ? x : null,
            ParameterSystemTypeEnum.Guid => Guid.TryParse(data.Value, out Guid x) ? x : null,
            ParameterSystemTypeEnum.DateTime => DateTime.TryParse(data.Value, out DateTime x) ? x : null,
            ParameterSystemTypeEnum.String => data.Value,
            _ => null,
        };

        if (parsed is not T typed) return false;
        value = typed;
        return true;
    }

    public bool TryGetParameterTextValue(string identifier, [NotNullWhen(true)] out string? value)
    {
        value = null;

        if (!TryGetParameterText(identifier, out EntityDataParameterText? text))
            return false;

        value = text.Value;
        return true;
    }

    private bool TryGetTyped<T>(string identifier, out T? typed) where T : EntityDataParameter
    {
        typed = null;

        if (!TryResolve(identifier, out _, out EntityDataParameter.Discriminator? data))
            return false;

        EntityDataParameter value = data.GetValue();
        if (value is not T t) return false;

        typed = t;
        return true;
    }

    private bool TryResolve(string identifier, [NotNullWhen(true)] out EntitySchemaParameter.Discriminator? schema, [NotNullWhen(true)] out EntityDataParameter.Discriminator? data)
    {
        schema = null;
        data = null;
        if (!_parameters.TryGetValue(identifier, out ParameterPair? pair)) return false;

        schema = pair.Schema;
        data = pair.Data;
        return true;
    }

    private sealed record ParameterPair(EntitySchemaParameter.Discriminator Schema, EntityDataParameter.Discriminator Data);
}

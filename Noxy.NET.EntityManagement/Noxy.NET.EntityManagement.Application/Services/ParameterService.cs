using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class ParameterService : IParameterService
{
    private readonly record struct ParameterKey(int Type, string Identifier);

    private readonly Lock _lock = new();
    private readonly Dictionary<ParameterKey, List<EntityDataParameter>> _cache = new();
    private readonly HashSet<string> _listValidSchemaIdentifier = [];

    private const int TypeCodeStyle = 1;
    private const int TypeCodeSystem = 2;
    private const int TypeCodeText = 3;

    public void Initialize(List<EntitySchemaParameter> listSchemaParameter, List<EntityDataParameter> listDataParameter)
    {
        lock (_lock)
        {
            _cache.Clear();
            _listValidSchemaIdentifier.Clear();

            foreach (EntitySchemaParameter schema in listSchemaParameter)
            {
                _listValidSchemaIdentifier.Add(schema.SchemaIdentifier);
            }

            foreach (EntityDataParameter parameter in listDataParameter)
            {
                ParameterKey key = CreateKey(parameter);
                if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list))
                {
                    _cache[key] = list = [];
                }

                list.Add(parameter);
            }

            foreach (ParameterKey key in _cache.Keys.ToList())
            {
                _cache[key].Sort(SortByEffectiveThenCreated);
            }
        }
    }

    public void AddToCache(EntityDataParameter parameter)
    {
        lock (_lock)
        {
            ParameterKey key = CreateKey(parameter);
            if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list))
            {
                _cache[key] = list = [];
            }

            list.Add(parameter);
            list.Sort(SortByEffectiveThenCreated);
        }
    }

    public void ReplaceInCache(EntityDataParameter parameter)
    {
        lock (_lock)
        {
            ParameterKey key = CreateKey(parameter);
            if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list))
            {
                _cache[key] = [parameter];
                return;
            }

            list.RemoveAll(x => x.ID == parameter.ID);
            list.Add(parameter);
            list.Sort(SortByEffectiveThenCreated);
        }
    }

    public void RemoveFromCache(EntityDataParameter parameter)
    {
        lock (_lock)
        {
            ParameterKey key = CreateKey(parameter);
            if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list)) return;

            list.Remove(parameter);
            if (list.Count == 0) _cache.Remove(key);
        }
    }

    public void UpdateValidSchemaIdentifierList(IEnumerable<EntitySchemaParameter> list)
    {
        lock (_lock)
        {
            _listValidSchemaIdentifier.Clear();
            foreach (EntitySchemaParameter schema in list)
            {
                _listValidSchemaIdentifier.Add(schema.SchemaIdentifier);
            }
        }
    }

    public bool TryGetParameterStyle(string identifier, [NotNullWhen(true)] out EntityDataParameterStyle? parameter)
    {
        parameter = GetEffective<EntityDataParameterStyle>(identifier);
        return parameter != null;
    }

    public bool TryGetParameterSystem(string identifier, [NotNullWhen(true)] out EntityDataParameterSystem? parameter)
    {
        parameter = GetEffective<EntityDataParameterSystem>(identifier);
        return parameter != null;
    }

    public bool TryGetParameterText(string identifier, [NotNullWhen(true)] out EntityDataParameterText? parameter)
    {
        parameter = GetEffective<EntityDataParameterText>(identifier);
        return parameter != null;
    }

    public bool TryGetParameterStyleValue(string identifier, [NotNullWhen(true)] out string? value) => TryGetParameterValue<EntityDataParameterStyle>(identifier, out value);

    public bool TryGetParameterSystemValue(string identifier, [NotNullWhen(true)] out string? value) => TryGetParameterValue<EntityDataParameterSystem>(identifier, out value);

    public bool TryGetParameterTextValue(string identifier, [NotNullWhen(true)] out string? value) => TryGetParameterValue<EntityDataParameterText>(identifier, out value);

    public bool TryGetParameterSystemValueBoolean(string identifier, out bool value) => TryGetSystemValue(identifier, ParameterSystemTypeEnum.Boolean, out value);

    public bool TryGetParameterSystemValueInt(string identifier, out int value) => TryGetSystemValue(identifier, ParameterSystemTypeEnum.Integer, out value);

    public bool TryGetParameterSystemValueDecimal(string identifier, out decimal value) => TryGetSystemValue(identifier, ParameterSystemTypeEnum.Decimal, out value);

    public bool TryGetParameterSystemValueGuid(string identifier, out Guid value) => TryGetSystemValue(identifier, ParameterSystemTypeEnum.Guid, out value);

    public bool TryGetParameterSystemValueDateTime(string identifier, out DateTime value) => TryGetSystemValue(identifier, ParameterSystemTypeEnum.DateTime, out value);

    public bool TryGetParameterSystemValueString(string identifier, out string? value) => TryGetSystemValue(identifier, ParameterSystemTypeEnum.String, out value);

    private T? GetEffective<T>(string identifier) where T : EntityDataParameter
    {
        lock (_lock)
        {
            if (!_listValidSchemaIdentifier.Contains(identifier)) return null;

            ParameterKey key = CreateKey<T>(identifier);
            if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list)) return null;

            DateTime now = DateTime.UtcNow;
            return list.FirstOrDefault(x => x.TimeApproved != null && x.TimeEffective <= now) as T;
        }
    }

    private bool TryGetParameterValue<TParam>(string identifier, [NotNullWhen(true)] out string? value) where TParam : EntityDataParameter
    {
        value = null;
        TParam? p = GetEffective<TParam>(identifier);
        if (p == null) return false;

        value = p.Value;
        return true;
    }

    private bool TryGetSystemValue<T>(string identifier, ParameterSystemTypeEnum type, out T? value)
    {
        value = default;
        if (!TryGetParameterSystemValue(identifier, out string? result)) return false;
        value = ParseSystemValue<T>(type, result);
        return true;
    }

    private static T? ParseSystemValue<T>(ParameterSystemTypeEnum type, string value)
    {
        object? parsed = type switch
        {
            ParameterSystemTypeEnum.Boolean => bool.TryParse(value, out bool b) ? b : null,
            ParameterSystemTypeEnum.Integer => int.TryParse(value, out int i) ? i : null,
            ParameterSystemTypeEnum.Decimal => decimal.TryParse(value, out decimal d) ? d : null,
            ParameterSystemTypeEnum.Guid => Guid.TryParse(value, out Guid g) ? g : null,
            ParameterSystemTypeEnum.DateTime => DateTime.TryParse(value, out DateTime dt) ? dt : null,
            ParameterSystemTypeEnum.String => value,
            _ => null
        };

        return parsed is T casted ? casted : default;
    }

    private static ParameterKey CreateKey(EntityDataParameter p)
    {
        return p switch
        {
            EntityDataParameterStyle => new(TypeCodeStyle, p.SchemaIdentifier),
            EntityDataParameterSystem => new(TypeCodeSystem, p.SchemaIdentifier),
            EntityDataParameterText => new(TypeCodeText, p.SchemaIdentifier),
            _ => throw new InvalidOperationException()
        };
    }

    private static ParameterKey CreateKey<T>(string identifier) where T : EntityDataParameter
    {
        return typeof(T) switch
        {
            { } t when t == typeof(EntityDataParameterStyle) => new(TypeCodeStyle, identifier),
            { } t when t == typeof(EntityDataParameterSystem) => new(TypeCodeSystem, identifier),
            { } t when t == typeof(EntityDataParameterText) => new(TypeCodeText, identifier),
            _ => throw new InvalidOperationException()
        };
    }

    private static readonly Comparison<EntityDataParameter> SortByEffectiveThenCreated =
        (a, b) =>
        {
            int comparison = b.TimeEffective.CompareTo(a.TimeEffective);
            return comparison != 0 ? comparison : b.TimeCreated.CompareTo(a.TimeCreated);
        };
}

using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class ParameterService(IUnitOfWorkFactory uowFactory) : IParameterService
{
    private readonly record struct ParameterKey(Type Type, string Identifier);

    private readonly Lock _lock = new();
    private readonly Dictionary<ParameterKey, List<EntityDataParameter>> _cache = new();

    public async Task Initialize()
    {
        IUnitOfWork uow = await uowFactory.Create();
        List<EntityDataParameter> list = await uow.Data.GetParameterList();

        lock (_lock)
        {
            _cache.Clear();

            foreach (EntityDataParameter p in list)
            {
                ParameterKey key = p switch
                {
                    EntityDataParameterStyle => new(typeof(EntityDataParameterStyle), p.SchemaIdentifier),
                    EntityDataParameterSystem => new(typeof(EntityDataParameterSystem), p.SchemaIdentifier),
                    EntityDataParameterText => new(typeof(EntityDataParameterText), p.SchemaIdentifier),
                    _ => throw new InvalidOperationException()
                };

                if (!_cache.TryGetValue(key, out List<EntityDataParameter>? bucket))
                {
                    _cache[key] = bucket = [];
                }

                bucket.Add(p);
            }

            foreach (ParameterKey key in _cache.Keys.ToList())
            {
                _cache[key] = _cache[key]
                    .OrderByDescending(x => x.TimeEffective)
                    .ThenByDescending(x => x.TimeCreated)
                    .ToList();
            }
        }
    }

    public void AddToCache(EntityDataParameter parameter)
    {
        lock (_lock)
        {
            ParameterKey key = parameter switch
            {
                EntityDataParameterStyle => new(typeof(EntityDataParameterStyle), parameter.SchemaIdentifier),
                EntityDataParameterSystem => new(typeof(EntityDataParameterSystem), parameter.SchemaIdentifier),
                EntityDataParameterText => new(typeof(EntityDataParameterText), parameter.SchemaIdentifier),
                _ => throw new InvalidOperationException()
            };

            if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list))
            {
                _cache[key] = list = [];
            }

            list.Add(parameter);
            list.Sort((a, b) =>
            {
                int comparison = b.TimeEffective.CompareTo(a.TimeEffective);
                return comparison != 0 ? comparison : b.TimeCreated.CompareTo(a.TimeCreated);
            });
        }
    }

    public void RemoveFromCache(EntityDataParameter parameter)
    {
        lock (_lock)
        {
            ParameterKey key = parameter switch
            {
                EntityDataParameterStyle => new(typeof(EntityDataParameterStyle), parameter.SchemaIdentifier),
                EntityDataParameterSystem => new(typeof(EntityDataParameterSystem), parameter.SchemaIdentifier),
                EntityDataParameterText => new(typeof(EntityDataParameterText), parameter.SchemaIdentifier),
                _ => throw new InvalidOperationException()
            };
            if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list)) return;

            list.Remove(parameter);
            if (list.Count == 0) _cache.Remove(key);
        }
    }


    private T? GetEffective<T>(string identifier) where T : EntityDataParameter
    {
        ParameterKey key = new(typeof(T), identifier);
        if (!_cache.TryGetValue(key, out List<EntityDataParameter>? list)) return null;

        DateTime now = DateTime.UtcNow;
        return list.FirstOrDefault(x => x.TimeApproved != null && x.TimeEffective <= now) as T;
    }

    public bool TryGetParameterStyle(string identifier, [NotNullWhen(true)] out EntityDataParameterStyle? parameter)
    {
        lock (_lock)
        {
            parameter = GetEffective<EntityDataParameterStyle>(identifier);
            return parameter != null;
        }
    }

    public bool TryGetParameterSystem(string identifier, [NotNullWhen(true)] out EntityDataParameterSystem? parameter)
    {
        lock (_lock)
        {
            parameter = GetEffective<EntityDataParameterSystem>(identifier);
            return parameter != null;
        }
    }

    public bool TryGetParameterText(string identifier, [NotNullWhen(true)] out EntityDataParameterText? parameter)
    {
        lock (_lock)
        {
            parameter = GetEffective<EntityDataParameterText>(identifier);
            return parameter != null;
        }
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

    private bool TryGetParameterValue<TParam>(string identifier, [NotNullWhen(true)] out string? value) where TParam : EntityDataParameter
    {
        value = null;

        lock (_lock)
        {
            TParam? p = GetEffective<TParam>(identifier);
            if (p == null) return false;

            value = p.Value;
            return true;
        }
    }

    private bool TryGetSystemValue<T>(string identifier, ParameterSystemTypeEnum type, out T? value)
    {
        value = default;
        if (!TryGetParameterSystemValue(identifier, out string? result)) return false;
        value = ParseSystemValue<T>(type, result);
        return true;
    }

    public static T? ParseSystemValue<T>(ParameterSystemTypeEnum type, string value)
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
}

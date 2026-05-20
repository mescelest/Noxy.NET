using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IParameterService
{
    Task Initialize();
    void AddToCache(EntityDataParameter parameter);
    void RemoveFromCache(EntityDataParameter parameter);

    bool TryGetParameterStyle(string identifier, [NotNullWhen(true)] out EntityDataParameterStyle? parameter);
    bool TryGetParameterSystem(string identifier, [NotNullWhen(true)] out EntityDataParameterSystem? parameter);
    bool TryGetParameterText(string identifier, [NotNullWhen(true)] out EntityDataParameterText? parameter);

    bool TryGetParameterStyleValue(string identifier, [NotNullWhen(true)] out string? value);
    bool TryGetParameterSystemValue(string identifier, [NotNullWhen(true)] out string? value);
    bool TryGetParameterTextValue(string identifier, [NotNullWhen(true)] out string? value);

    bool TryGetParameterSystemValueBoolean(string identifier, out bool value);
    bool TryGetParameterSystemValueInt(string identifier, out int value);
    bool TryGetParameterSystemValueDecimal(string identifier, out decimal value);
    bool TryGetParameterSystemValueGuid(string identifier, out Guid value);
    bool TryGetParameterSystemValueDateTime(string identifier, out DateTime value);
    bool TryGetParameterSystemValueString(string identifier, out string? value);
}

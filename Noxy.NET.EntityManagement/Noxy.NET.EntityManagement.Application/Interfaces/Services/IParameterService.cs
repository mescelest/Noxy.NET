using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IParameterService
{
    void SetParameter(EntitySchemaParameter.Discriminator parameterSchema, EntityDataParameter.Discriminator parameterData);
    void SetParameterCollection(Dictionary<EntitySchemaParameter.Discriminator, EntityDataParameter.Discriminator> collection);

    bool TryGetParameter(string identifier, [NotNullWhen(true)] out EntityDataParameter.Discriminator? parameter);
    bool TryGetParameterStyle(string identifier, [NotNullWhen(true)] out EntityDataParameterStyle? parameter);
    bool TryGetParameterSystem(string identifier, [NotNullWhen(true)] out EntityDataParameterSystem? parameter);
    bool TryGetParameterText(string identifier, [NotNullWhen(true)] out EntityDataParameterText? parameter);

    bool TryGetParameterStyleValue(string identifier, [NotNullWhen(true)] out string? value);
    bool TryGetParameterSystemValue<T>(string identifier, [NotNullWhen(true)] out T? value);
    bool TryGetParameterTextValue(string identifier, [NotNullWhen(true)] out string? value);
}

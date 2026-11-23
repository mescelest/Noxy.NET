using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.Test.EntityManagement.Definitions;

// A definition written inline in a test.
internal sealed class ActionDefinition(Action<IDefinitionBuilder> define) : IDefinition
{
    public void Define(IDefinitionBuilder builder) => define(builder);
}

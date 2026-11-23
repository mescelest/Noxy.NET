using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.Test.EntityManagement.Application;

// A file of texts written inline in a test.
internal sealed class ActionTexts(Action<IDefinitionTextBuilder> define) : IDefinitionTexts
{
    public void Define(IDefinitionTextBuilder builder) => define(builder);
}

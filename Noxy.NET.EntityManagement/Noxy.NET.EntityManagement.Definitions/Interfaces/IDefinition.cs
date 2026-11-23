namespace Noxy.NET.EntityManagement.Definitions.Interfaces;

// The schema of a system, written as calls on the builder. A project has one or more of these, and they are run once at startup.
public interface IDefinition
{
    void Define(IDefinitionBuilder builder);
}

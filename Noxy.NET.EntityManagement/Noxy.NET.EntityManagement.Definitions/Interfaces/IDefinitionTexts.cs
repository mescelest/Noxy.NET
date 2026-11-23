namespace Noxy.NET.EntityManagement.Definitions.Interfaces;

// What the texts say, in each language the system ships, written as calls on the builder. Like the definition, it is run once at startup.
// Several files can be registered, for example one for each language, or one from a base system and one from the project that builds on it.
public interface IDefinitionTexts
{
    void Define(IDefinitionTextBuilder builder);
}

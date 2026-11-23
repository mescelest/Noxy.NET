using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.EntityManagement.Application;

// What the system is made of: the definitions (the schema in code) and the texts. Both are run when the services are set up, so a mistake in them stops the application at startup, with every problem listed.
public sealed class EntityManagementApplicationOptions
{
    private readonly List<IDefinition> _definitionList = [];
    private readonly List<IDefinitionTexts> _textsList = [];

    // The culture a text falls back to when it has nothing in the culture that is asked for. Every text must have a default in it.
    public string FallbackCulture { get; set; } = DefinitionTextBuilder.DefaultFallbackCulture;

    public EntityManagementApplicationOptions AddDefinition(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        _definitionList.Add(definition);
        return this;
    }

    public EntityManagementApplicationOptions AddDefinition<TDefinition>() where TDefinition : IDefinition, new()
    {
        return AddDefinition(new TDefinition());
    }

    public EntityManagementApplicationOptions AddTexts(IDefinitionTexts texts)
    {
        ArgumentNullException.ThrowIfNull(texts);

        _textsList.Add(texts);
        return this;
    }

    public EntityManagementApplicationOptions AddTexts<TTexts>() where TTexts : IDefinitionTexts, new()
    {
        return AddTexts(new TTexts());
    }

    internal DefinitionRegistry BuildDefinition()
    {
        return DefinitionBuilder.Create([.. _definitionList]);
    }

    internal DefinitionTextRegistry BuildTexts(DefinitionRegistry definition)
    {
        return DefinitionTextBuilder.Create(definition, FallbackCulture, [.. _textsList]);
    }
}

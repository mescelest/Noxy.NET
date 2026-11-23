using System.Diagnostics.CodeAnalysis;
using Noxy.NET.EntityManagement.Definitions.Models;

namespace Noxy.NET.EntityManagement.Definitions;

// The checked and read-only result of the definitions. Everything in it refers to something that exists, so nothing needs to be checked again.
public sealed class DefinitionRegistry
{
    private readonly Dictionary<string, DefinitionTextParameter> _textParameterMap;
    private readonly Dictionary<string, DefinitionProperty> _propertyMap;
    private readonly Dictionary<string, DefinitionElement> _elementMap;
    private readonly Dictionary<string, DefinitionRelation> _relationMap;
    private readonly Dictionary<string, DefinitionContext> _contextMap;

    public IReadOnlyList<DefinitionTextParameter> TextParameterList { get; }
    public IReadOnlyList<DefinitionProperty> PropertyList { get; }
    public IReadOnlyList<DefinitionElement> ElementList { get; }
    public IReadOnlyList<DefinitionRelation> RelationList { get; }
    public IReadOnlyList<DefinitionContext> ContextList { get; }

    internal DefinitionRegistry(
        List<DefinitionTextParameter> textParameterList,
        List<DefinitionProperty> propertyList,
        List<DefinitionElement> elementList,
        List<DefinitionRelation> relationList,
        List<DefinitionContext> contextList)
    {
        TextParameterList = textParameterList.AsReadOnly();
        PropertyList = propertyList.AsReadOnly();
        ElementList = elementList.AsReadOnly();
        RelationList = relationList.AsReadOnly();
        ContextList = contextList.AsReadOnly();

        _textParameterMap = textParameterList.ToDictionary(x => x.Identifier);
        _propertyMap = propertyList.ToDictionary(x => x.Identifier);
        _elementMap = elementList.ToDictionary(x => x.Identifier);
        _relationMap = relationList.ToDictionary(x => x.Identifier);
        _contextMap = contextList.ToDictionary(x => x.Identifier);
    }

    public DefinitionTextParameter GetTextParameter(string identifier) => Get(_textParameterMap, identifier, "text parameter");
    public DefinitionProperty GetProperty(string identifier) => Get(_propertyMap, identifier, "property");
    public DefinitionElement GetElement(string identifier) => Get(_elementMap, identifier, "element");
    public DefinitionRelation GetRelation(string identifier) => Get(_relationMap, identifier, "relation");
    public DefinitionContext GetContext(string identifier) => Get(_contextMap, identifier, "context");

    public bool TryGetTextParameter(string identifier, [NotNullWhen(true)] out DefinitionTextParameter? definition) => _textParameterMap.TryGetValue(identifier, out definition);
    public bool TryGetProperty(string identifier, [NotNullWhen(true)] out DefinitionProperty? definition) => _propertyMap.TryGetValue(identifier, out definition);
    public bool TryGetElement(string identifier, [NotNullWhen(true)] out DefinitionElement? definition) => _elementMap.TryGetValue(identifier, out definition);
    public bool TryGetRelation(string identifier, [NotNullWhen(true)] out DefinitionRelation? definition) => _relationMap.TryGetValue(identifier, out definition);
    public bool TryGetContext(string identifier, [NotNullWhen(true)] out DefinitionContext? definition) => _contextMap.TryGetValue(identifier, out definition);

    private static T Get<T>(Dictionary<string, T> map, string identifier, string kindName)
    {
        return map.TryGetValue(identifier, out T? definition) ? definition : throw new KeyNotFoundException($"There is no {kindName} '{identifier}'.");
    }
}

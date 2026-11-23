using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Definitions.Models;

namespace Noxy.NET.EntityManagement.Definitions;

public sealed class DefinitionBuilder : IDefinitionBuilder
{
    private readonly List<DefinitionTextParameter> _textParameterList = [];
    private readonly List<DefinitionProperty> _propertyList = [];
    private readonly List<DefinitionElement> _elementList = [];
    private readonly List<DefinitionRelation> _relationList = [];
    private readonly List<DefinitionContext> _contextList = [];

    // Runs the definitions (the project's own, and a base system's if there is one) and builds them as one.
    public static DefinitionRegistry Create(params IDefinition[] definitionList)
    {
        DefinitionBuilder builder = new();
        foreach (IDefinition definition in definitionList)
        {
            builder.Add(definition);
        }

        return builder.Build();
    }

    public DefinitionBuilder Add(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        definition.Define(this);
        return this;
    }

    public IDefinitionBuilder AddTextParameter(string identifier, TextKindEnum kind)
    {
        _textParameterList.Add(new(identifier, kind));
        return this;
    }

    public IDefinitionBuilder AddProperty(string identifier, PropertyKindEnum kind, string titleText, string? descriptionText = null, bool isRequired = false, int? maxLength = null)
    {
        _propertyList.Add(new(identifier, kind, titleText, descriptionText, isRequired, maxLength));
        return this;
    }

    public IDefinitionBuilder AddElement(string identifier, string titleText, IEnumerable<string> properties, string? descriptionText = null)
    {
        ArgumentNullException.ThrowIfNull(properties);

        _elementList.Add(new(identifier, titleText, descriptionText, [.. properties]));
        return this;
    }

    public IDefinitionBuilder AddRelation(
        string identifier,
        string fromElement,
        string toElement,
        string forwardTitleText,
        string inverseTitleText,
        RelationCardinalityEnum cardinality = RelationCardinalityEnum.ManyToMany,
        IEnumerable<string>? attributes = null,
        string? descriptionText = null)
    {
        _relationList.Add(new(identifier, fromElement, toElement, forwardTitleText, inverseTitleText, cardinality, [.. attributes ?? []], descriptionText));
        return this;
    }

    public IDefinitionBuilder AddContext(string identifier, string titleText, IEnumerable<string> elements, string? descriptionText = null)
    {
        ArgumentNullException.ThrowIfNull(elements);

        _contextList.Add(new(identifier, titleText, descriptionText, [.. elements]));
        return this;
    }

    // Checks everything and either returns the read-only registry, or throws with every problem found.
    public DefinitionRegistry Build()
    {
        List<string> errorList = [];

        List<DefinitionTextParameter> textParameterList = Index(_textParameterList, x => x.Identifier, "Text parameter", errorList);
        List<DefinitionProperty> propertyList = Index(_propertyList, x => x.Identifier, "Property", errorList);
        List<DefinitionElement> elementList = Index(_elementList, x => x.Identifier, "Element", errorList);
        List<DefinitionRelation> relationList = Index(_relationList, x => x.Identifier, "Relation", errorList);
        List<DefinitionContext> contextList = Index(_contextList, x => x.Identifier, "Context", errorList);

        HashSet<string> textSet = textParameterList.Select(x => x.Identifier).ToHashSet();
        HashSet<string> propertySet = propertyList.Select(x => x.Identifier).ToHashSet();
        HashSet<string> elementSet = elementList.Select(x => x.Identifier).ToHashSet();

        foreach (DefinitionProperty property in propertyList)
        {
            string owner = $"Property '{property.Identifier}'";
            CheckText(property.TitleText, textSet, owner, "title", true, errorList);
            CheckText(property.DescriptionText, textSet, owner, "description", false, errorList);

            if (!property.MaxLength.HasValue) continue;

            if (property.Kind != PropertyKindEnum.String)
            {
                errorList.Add($"{owner}: a maximum length only applies to a String property, not {property.Kind}.");
            }
            else if (property.MaxLength.Value < 1)
            {
                errorList.Add($"{owner}: the maximum length must be at least 1.");
            }
        }

        foreach (DefinitionElement element in elementList)
        {
            string owner = $"Element '{element.Identifier}'";
            CheckText(element.TitleText, textSet, owner, "title", true, errorList);
            CheckText(element.DescriptionText, textSet, owner, "description", false, errorList);
            CheckReferenceList(element.PropertyList, propertySet, owner, "property", errorList);
        }

        foreach (DefinitionRelation relation in relationList)
        {
            string owner = $"Relation '{relation.Identifier}'";
            CheckText(relation.ForwardTitleText, textSet, owner, "forward title", true, errorList);
            CheckText(relation.InverseTitleText, textSet, owner, "inverse title", true, errorList);
            CheckText(relation.DescriptionText, textSet, owner, "description", false, errorList);
            CheckReferenceList([relation.FromElement], elementSet, owner, "element (from)", errorList);
            CheckReferenceList([relation.ToElement], elementSet, owner, "element (to)", errorList);
            CheckReferenceList(relation.AttributeList, propertySet, owner, "attribute property", errorList);
        }

        foreach (DefinitionContext context in contextList)
        {
            string owner = $"Context '{context.Identifier}'";
            CheckText(context.TitleText, textSet, owner, "title", true, errorList);
            CheckText(context.DescriptionText, textSet, owner, "description", false, errorList);
            CheckReferenceList(context.ElementList, elementSet, owner, "element", errorList);

            if (context.ElementList.Count == 0)
            {
                errorList.Add($"{owner}: a context needs at least one element.");
            }
        }

        if (errorList.Count > 0)
        {
            throw new DefinitionException(errorList);
        }

        return new(textParameterList, propertyList, elementList, relationList, contextList);
    }

    // The definitions of one kind, in the order they were added. A repeated or badly written identifier is reported once and the definition is kept, so what refers to it is not reported as missing too.
    private static List<T> Index<T>(List<T> source, Func<T, string> identifierOf, string kindName, List<string> errorList)
    {
        List<T> result = [];
        HashSet<string> seen = [];

        foreach (T item in source)
        {
            string identifier = identifierOf(item);

            if (!DefinitionIdentifier.IsValid(identifier))
            {
                errorList.Add($"{kindName} '{identifier}': the identifier is not valid (lower case letters and digits, separated by single '-' or '.', at most {DefinitionIdentifier.MaxLength} characters).");
            }

            if (!seen.Add(identifier))
            {
                errorList.Add($"{kindName} '{identifier}': defined more than once.");
                continue;
            }

            result.Add(item);
        }

        return result;
    }

    private static void CheckText(string? identifier, HashSet<string> textSet, string owner, string role, bool isRequired, List<string> errorList)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            if (isRequired)
            {
                errorList.Add($"{owner}: a {role} text parameter is required.");
            }

            return;
        }

        if (!textSet.Contains(identifier))
        {
            errorList.Add($"{owner}: the {role} text parameter '{identifier}' is not defined.");
        }
    }

    private static void CheckReferenceList(IReadOnlyList<string> identifierList, HashSet<string> knownSet, string owner, string kindName, List<string> errorList)
    {
        HashSet<string> seen = [];

        foreach (string identifier in identifierList)
        {
            if (!knownSet.Contains(identifier))
            {
                errorList.Add($"{owner}: {kindName} '{identifier}' is not defined.");
            }
            else if (!seen.Add(identifier))
            {
                errorList.Add($"{owner}: {kindName} '{identifier}' is listed more than once.");
            }
        }
    }
}

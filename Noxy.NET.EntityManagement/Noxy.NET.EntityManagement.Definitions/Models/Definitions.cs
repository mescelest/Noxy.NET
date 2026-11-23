using Noxy.NET.EntityManagement.Definitions.Enums;

namespace Noxy.NET.EntityManagement.Definitions.Models;

// A text a user will see: that it exists, and what kind it is. What it says is not here (see DefinitionText for the defaults, and the values in the database).
public sealed record DefinitionTextParameter(string Identifier, TextKindEnum Kind);

// What a text says in one culture, as the system ships it. These are defaults: a person can edit them, and the edit is kept in the database.
public sealed record DefinitionText(string TextIdentifier, string Culture, string Value);

// A typed field. TitleText and DescriptionText are identifiers of text parameters.
public sealed record DefinitionProperty(string Identifier, PropertyKindEnum Kind, string TitleText, string? DescriptionText, bool IsRequired, int? MaxLength);

// A kind of thing, made of properties (identifiers, in the order they are shown).
public sealed record DefinitionElement(string Identifier, string TitleText, string? DescriptionText, IReadOnlyList<string> PropertyList);

// A link between two kinds of element. The forward title reads from the first element to the second ("Involved in"), the inverse title the other way ("Involved persons"). The attributes are properties of the link itself.
public sealed record DefinitionRelation(
    string Identifier,
    string FromElement,
    string ToElement,
    string ForwardTitleText,
    string InverseTitleText,
    RelationCardinalityEnum Cardinality,
    IReadOnlyList<string> AttributeList,
    string? DescriptionText);

// A group of elements that belong together, and so the data to query for a page.
public sealed record DefinitionContext(string Identifier, string TitleText, string? DescriptionText, IReadOnlyList<string> ElementList);

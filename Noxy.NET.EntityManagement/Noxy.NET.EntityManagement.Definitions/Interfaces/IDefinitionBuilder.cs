using Noxy.NET.EntityManagement.Definitions.Enums;

namespace Noxy.NET.EntityManagement.Definitions.Interfaces;

// The calls only collect. Nothing is checked until the builder is built, so they can be made in any order.
// Every title and description is the identifier of a text parameter, because everything a user sees can be translated.
// A text parameter only says that a text exists and what kind it is. What it says, in each language, is registered apart (see IDefinitionTexts).
public interface IDefinitionBuilder
{
    IDefinitionBuilder AddTextParameter(string identifier, TextKindEnum kind);

    IDefinitionBuilder AddProperty(string identifier, PropertyKindEnum kind, string titleText, string? descriptionText = null, bool isRequired = false, int? maxLength = null);

    IDefinitionBuilder AddElement(string identifier, string titleText, IEnumerable<string> properties, string? descriptionText = null);

    IDefinitionBuilder AddRelation(
        string identifier,
        string fromElement,
        string toElement,
        string forwardTitleText,
        string inverseTitleText,
        RelationCardinalityEnum cardinality = RelationCardinalityEnum.ManyToMany,
        IEnumerable<string>? attributes = null,
        string? descriptionText = null);

    IDefinitionBuilder AddContext(string identifier, string titleText, IEnumerable<string> elements, string? descriptionText = null);
}

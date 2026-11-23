namespace Noxy.NET.EntityManagement.Definitions.Interfaces;

// The calls only collect. Nothing is checked until the texts are built against the definition, so they can be made in any order.
public interface IDefinitionTextBuilder
{
    // The default of a text in a culture. A text can have one for each language. A person can edit it afterwards; the edit is kept in the database and is never replaced by a newer default.
    IDefinitionTextBuilder AddText(string textIdentifier, string culture, string value);
}

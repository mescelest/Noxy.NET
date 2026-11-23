using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Demo.Definitions.Constants;

namespace Noxy.NET.EntityManagement.Demo.Definitions.Texts;

// The texts in English. A language is a file like this one: add another file, and register it next to this one at startup.
public sealed class CaseworkEnglishTexts : IDefinitionTexts
{
    public const string Culture = "en";

    public void Define(IDefinitionTextBuilder builder)
    {
        builder.AddText(CaseworkTexts.PersonTitle, Culture, "Person");
        builder.AddText(CaseworkTexts.CaseTitle, Culture, "Case");
        builder.AddText(CaseworkTexts.PeopleContextTitle, Culture, "People");
        builder.AddText(CaseworkTexts.CaseFileContextTitle, Culture, "Case file");
        builder.AddText(CaseworkTexts.PropertyNameTitle, Culture, "Name");
        builder.AddText(CaseworkTexts.PropertyPictureTitle, Culture, "Picture");
        builder.AddText(CaseworkTexts.PropertyAgeTitle, Culture, "Age");
        builder.AddText(CaseworkTexts.PropertyRoleTitle, Culture, "Role");
        builder.AddText(CaseworkTexts.PropertyNoteTitle, Culture, "Note");
        builder.AddText(CaseworkTexts.PersonInvolvedInCaseForwardTitle, Culture, "Involved in");
        builder.AddText(CaseworkTexts.PersonInvolvedInCaseInverseTitle, Culture, "Involved people");
    }
}

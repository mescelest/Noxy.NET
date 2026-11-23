using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Demo.Definitions.Constants;

namespace Noxy.NET.EntityManagement.Demo.Definitions;

// The schema of the demo system, run once at startup. The person and the case, and the relation between them, are here for now; vehicles, weapons and locations come later.
public sealed class CaseworkDefinition : IDefinition
{
    public void Define(IDefinitionBuilder builder)
    {
        builder.AddTextParameter(CaseworkTexts.PersonTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.CaseTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PeopleContextTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.CaseFileContextTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PropertyNameTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PropertyPictureTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PropertyAgeTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PropertyRoleTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PropertyNoteTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PersonInvolvedInCaseForwardTitle, TextKindEnum.Line);
        builder.AddTextParameter(CaseworkTexts.PersonInvolvedInCaseInverseTitle, TextKindEnum.Line);

        builder.AddProperty(CaseworkProperties.Name, PropertyKindEnum.String, CaseworkTexts.PropertyNameTitle, isRequired: true, maxLength: 200);
        builder.AddProperty(CaseworkProperties.Picture, PropertyKindEnum.Image, CaseworkTexts.PropertyPictureTitle);
        builder.AddProperty(CaseworkProperties.Age, PropertyKindEnum.Integer, CaseworkTexts.PropertyAgeTitle);
        builder.AddProperty(CaseworkProperties.Role, PropertyKindEnum.String, CaseworkTexts.PropertyRoleTitle, isRequired: true, maxLength: 100);
        builder.AddProperty(CaseworkProperties.Note, PropertyKindEnum.String, CaseworkTexts.PropertyNoteTitle);

        builder.AddElement(CaseworkElements.Person, CaseworkTexts.PersonTitle, [CaseworkProperties.Name, CaseworkProperties.Picture, CaseworkProperties.Age]);
        builder.AddElement(CaseworkElements.Case, CaseworkTexts.CaseTitle, [CaseworkProperties.Name, CaseworkProperties.Note]);

        // A person can be involved in many cases, and a case has many people involved, each in a role of their own.
        builder.AddRelation(
            CaseworkRelations.PersonInvolvedInCase,
            CaseworkElements.Person,
            CaseworkElements.Case,
            CaseworkTexts.PersonInvolvedInCaseForwardTitle,
            CaseworkTexts.PersonInvolvedInCaseInverseTitle,
            RelationCardinalityEnum.ManyToMany,
            [CaseworkProperties.Role, CaseworkProperties.Note]);

        builder.AddContext(CaseworkContexts.People, CaseworkTexts.PeopleContextTitle, [CaseworkElements.Person]);
        builder.AddContext(CaseworkContexts.CaseFile, CaseworkTexts.CaseFileContextTitle, [CaseworkElements.Case, CaseworkElements.Person]);
    }
}

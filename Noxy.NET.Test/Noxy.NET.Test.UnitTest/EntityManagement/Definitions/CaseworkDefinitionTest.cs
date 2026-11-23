using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Demo.Definitions;
using Noxy.NET.EntityManagement.Demo.Definitions.Constants;
using Noxy.NET.EntityManagement.Demo.Definitions.Texts;

namespace Noxy.NET.Test.EntityManagement.Definitions;

public class CaseworkDefinitionTest
{
    private static DefinitionRegistry Build() => DefinitionBuilder.Create(new CaseworkDefinition());

    [Fact]
    public void TheDemoDefinition_IsValid()
    {
        DefinitionRegistry registry = Build();

        Assert.NotEmpty(registry.ElementList);
        Assert.NotEmpty(registry.ContextList);
    }

    [Fact]
    public void ThePersonHasANameThatIsRequired_APicture_AndAnAge()
    {
        DefinitionRegistry registry = Build();

        DefinitionElement person = registry.GetElement(CaseworkElements.Person);
        Assert.Equal([CaseworkProperties.Name, CaseworkProperties.Picture, CaseworkProperties.Age], person.PropertyList);

        DefinitionProperty name = registry.GetProperty(CaseworkProperties.Name);
        Assert.Equal(PropertyKindEnum.String, name.Kind);
        Assert.True(name.IsRequired);
        Assert.Equal(200, name.MaxLength);

        Assert.Equal(PropertyKindEnum.Image, registry.GetProperty(CaseworkProperties.Picture).Kind);
        Assert.Equal(PropertyKindEnum.Integer, registry.GetProperty(CaseworkProperties.Age).Kind);
    }

    [Fact]
    public void ThePersonIsInvolvedInACase_WithARequiredRoleAndAnOptionalNote()
    {
        DefinitionRegistry registry = Build();

        DefinitionRelation relation = registry.GetRelation(CaseworkRelations.PersonInvolvedInCase);

        Assert.Equal((CaseworkElements.Person, CaseworkElements.Case, RelationCardinalityEnum.ManyToMany), (relation.FromElement, relation.ToElement, relation.Cardinality));
        Assert.Equal([CaseworkProperties.Role, CaseworkProperties.Note], relation.AttributeList);
        Assert.True(registry.GetProperty(CaseworkProperties.Role).IsRequired);
        Assert.False(registry.GetProperty(CaseworkProperties.Note).IsRequired);
    }

    [Fact]
    public void TheCaseFileContext_GroupsTheCaseAndThePerson()
    {
        Assert.Equal([CaseworkElements.Case, CaseworkElements.Person], Build().GetContext(CaseworkContexts.CaseFile).ElementList);
    }

    [Fact]
    public void ThePeopleContext_GroupsThePerson()
    {
        Assert.Equal([CaseworkElements.Person], Build().GetContext(CaseworkContexts.People).ElementList);
    }

    [Fact]
    public void EveryTextOfTheDemo_HasAnEnglishText()
    {
        DefinitionRegistry definition = Build();
        DefinitionTextRegistry texts = DefinitionTextBuilder.Create(definition, new CaseworkEnglishTexts());

        Assert.Equal(["en"], texts.CultureList);
        Assert.All(definition.TextParameterList, x => Assert.True(texts.TryGetText(x.Identifier, CaseworkEnglishTexts.Culture, out _), $"{x.Identifier} has no English text."));

        Assert.True(texts.TryGetText(CaseworkTexts.PropertyNameTitle, "en", out DefinitionText? name));
        Assert.Equal("Name", name.Value);
    }

    [Fact]
    public void EveryTextConstant_IsDefined()
    {
        DefinitionRegistry registry = Build();

        IEnumerable<string> constantList = typeof(CaseworkTexts).GetFields().Where(x => x is { IsLiteral: true, IsInitOnly: false }).Select(x => (string)x.GetRawConstantValue()!);

        Assert.All(constantList, x => Assert.True(registry.TryGetTextParameter(x, out _), $"The text parameter '{x}' is a constant but is not defined."));
    }
}

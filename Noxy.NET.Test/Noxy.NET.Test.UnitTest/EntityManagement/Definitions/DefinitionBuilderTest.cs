using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Definitions.Models;

namespace Noxy.NET.Test.EntityManagement.Definitions;

public class DefinitionBuilderTest
{
    // A small valid definition that the tests change one thing in.
    private static void DefineValid(IDefinitionBuilder builder)
    {
        builder.AddTextParameter("person-title", TextKindEnum.Line);
        builder.AddTextParameter("name-title", TextKindEnum.Line);
        builder.AddTextParameter("role-title", TextKindEnum.Line);
        builder.AddTextParameter("case-title", TextKindEnum.Line);
        builder.AddTextParameter("involved-in-title", TextKindEnum.Line);
        builder.AddTextParameter("involved-persons-title", TextKindEnum.Line);
        builder.AddTextParameter("case-file-title", TextKindEnum.Line);

        builder.AddProperty("name", PropertyKindEnum.String, "name-title", isRequired: true, maxLength: 200);
        builder.AddProperty("role", PropertyKindEnum.String, "role-title");

        builder.AddElement("person", "person-title", ["name"]);
        builder.AddElement("case", "case-title", ["name"]);

        builder.AddRelation("person-involved-in-case", "person", "case", "involved-in-title", "involved-persons-title", attributes: ["role"]);

        builder.AddContext("case-file", "case-file-title", ["case", "person"]);
    }

    private static DefinitionException Fails(Action<IDefinitionBuilder> define)
    {
        return Assert.Throws<DefinitionException>(() => DefinitionBuilder.Create(new ActionDefinition(define)));
    }

    // The valid definition with one more call.
    private static Action<IDefinitionBuilder> WithExtra(Action<IDefinitionBuilder> extra)
    {
        return builder =>
        {
            DefineValid(builder);
            extra(builder);
        };
    }

    #region -- A valid definition --

    [Fact]
    public void AValidDefinition_IsBuilt_AndEverythingCanBeLookedUp()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(DefineValid));

        Assert.Equal(7, registry.TextParameterList.Count);
        Assert.Equal(["name", "role"], registry.PropertyList.Select(x => x.Identifier));
        Assert.Equal(["person", "case"], registry.ElementList.Select(x => x.Identifier));
        Assert.Single(registry.RelationList);
        Assert.Single(registry.ContextList);

        DefinitionProperty name = registry.GetProperty("name");
        Assert.Equal(PropertyKindEnum.String, name.Kind);
        Assert.True(name.IsRequired);
        Assert.Equal(200, name.MaxLength);
        Assert.Equal(TextKindEnum.Line, registry.GetTextParameter(name.TitleText).Kind);

        DefinitionRelation relation = registry.GetRelation("person-involved-in-case");
        Assert.Equal(("person", "case"), (relation.FromElement, relation.ToElement));
        Assert.Equal(RelationCardinalityEnum.ManyToMany, relation.Cardinality);
        Assert.Equal(["role"], relation.AttributeList);

        Assert.Equal(["case", "person"], registry.GetContext("case-file").ElementList);
    }

    [Fact]
    public void TheDefinitionsAreKeptInTheOrderTheyWereAdded()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddTextParameter("b-title", TextKindEnum.Line);
            builder.AddTextParameter("a-title", TextKindEnum.Line);
            builder.AddProperty("zeta", PropertyKindEnum.Integer, "b-title");
            builder.AddProperty("alpha", PropertyKindEnum.Integer, "a-title");
        }));

        Assert.Equal(["b-title", "a-title"], registry.TextParameterList.Select(x => x.Identifier));
        Assert.Equal(["zeta", "alpha"], registry.PropertyList.Select(x => x.Identifier));
    }

    [Fact]
    public void TheCallsCanBeMadeInAnyOrder()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddContext("people", "people-title", ["person"]);
            builder.AddElement("person", "person-title", ["name"]);
            builder.AddProperty("name", PropertyKindEnum.String, "name-title");
            builder.AddTextParameter("name-title", TextKindEnum.Line);
            builder.AddTextParameter("person-title", TextKindEnum.Line);
            builder.AddTextParameter("people-title", TextKindEnum.Line);
        }));

        Assert.Equal(["person"], registry.GetContext("people").ElementList);
    }

    [Fact]
    public void ADefinitionCanBeEmpty()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create();

        Assert.Empty(registry.ElementList);
        Assert.Empty(registry.ContextList);
    }

    [Fact]
    public void AnElementCanHaveNoPropertiesYet_AndAPropertyCanBelongToSeveralElements()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(WithExtra(builder =>
        {
            builder.AddElement("vehicle", "person-title", ["name"]);
            builder.AddElement("empty", "person-title", []);
        })));

        Assert.Equal(["name"], registry.GetElement("person").PropertyList);
        Assert.Equal(["name"], registry.GetElement("vehicle").PropertyList);
        Assert.Empty(registry.GetElement("empty").PropertyList);
    }

    [Fact]
    public void ARelationCanLinkAnElementToItself()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(WithExtra(builder => { builder.AddRelation("person-knows-person", "person", "person", "involved-in-title", "involved-persons-title"); })));

        Assert.Equal(("person", "person"), (registry.GetRelation("person-knows-person").FromElement, registry.GetRelation("person-knows-person").ToElement));
    }

    [Fact]
    public void TheSameIdentifier_IsAllowedForDifferentKindsOfDefinition()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddTextParameter("name", TextKindEnum.Line);
            builder.AddProperty("name", PropertyKindEnum.String, "name");
            builder.AddElement("name", "name", ["name"]);
            builder.AddContext("name", "name", ["name"]);
        }));

        Assert.NotNull(registry.GetTextParameter("name"));
        Assert.NotNull(registry.GetProperty("name"));
        Assert.NotNull(registry.GetElement("name"));
        Assert.NotNull(registry.GetContext("name"));
    }

    [Fact]
    public void SeveralDefinitions_AreBuiltAsOne_AndOneCanUseWhatAnotherDefines()
    {
        ActionDefinition baseSystem = new(builder =>
        {
            builder.AddTextParameter("name-title", TextKindEnum.Line);
            builder.AddProperty("name", PropertyKindEnum.String, "name-title");
        });
        ActionDefinition project = new(builder =>
        {
            builder.AddTextParameter("acme.weapon-title", TextKindEnum.Line);
            builder.AddElement("acme.weapon", "acme.weapon-title", ["name"]);
        });

        DefinitionRegistry registry = DefinitionBuilder.Create(baseSystem, project);

        Assert.Equal(["name"], registry.GetElement("acme.weapon").PropertyList);
    }

    [Fact]
    public void TheSameIdentifier_InTwoDefinitions_IsAConflict()
    {
        ActionDefinition first = new(builder => builder.AddTextParameter("name-title", TextKindEnum.Line));
        ActionDefinition second = new(builder => builder.AddTextParameter("name-title", TextKindEnum.Line));

        DefinitionException exception = Assert.Throws<DefinitionException>(() => DefinitionBuilder.Create(first, second));

        Assert.Equal(["Text parameter 'name-title': defined more than once."], exception.ErrorList);
    }

    [Fact]
    public void ALookupOfSomethingThatIsNotThere_ThrowsOrReturnsFalse()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(DefineValid));

        Assert.False(registry.TryGetElement("nothing", out DefinitionElement? element));
        Assert.Null(element);
        Assert.True(registry.TryGetElement("person", out element));
        Assert.Equal("person", element.Identifier);

        KeyNotFoundException exception = Assert.Throws<KeyNotFoundException>(() => registry.GetElement("nothing"));
        Assert.Contains("element 'nothing'", exception.Message);
    }

    #endregion -- A valid definition --

    #region -- Identifiers --

    [Theory]
    [InlineData("person")]
    [InlineData("a")]
    [InlineData("a1")]
    [InlineData("person-involved-in-case")]
    [InlineData("acme.weapon")]
    [InlineData("acme.weapon-registry")]
    [InlineData("v2")]
    public void AnIdentifier_OfLowerCaseLettersAndDigitsWithSingleSeparators_IsValid(string identifier)
    {
        Assert.True(DefinitionIdentifier.IsValid(identifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Person")]
    [InlineData("per son")]
    [InlineData("-person")]
    [InlineData("person-")]
    [InlineData(".person")]
    [InlineData("person.")]
    [InlineData("per--son")]
    [InlineData("per..son")]
    [InlineData("per-.son")]
    [InlineData("per_son")]
    [InlineData("personæ")]
    public void AnIdentifier_ThatIsEmptyOrOddlyWritten_IsNotValid(string? identifier)
    {
        Assert.False(DefinitionIdentifier.IsValid(identifier));
    }

    [Fact]
    public void AnIdentifier_IsLimitedInLength()
    {
        Assert.True(DefinitionIdentifier.IsValid(new('a', DefinitionIdentifier.MaxLength)));
        Assert.False(DefinitionIdentifier.IsValid(new('a', DefinitionIdentifier.MaxLength + 1)));
    }

    [Fact]
    public void AnInvalidIdentifier_IsReported_OnceForEachKind()
    {
        DefinitionException exception = Fails(builder =>
        {
            builder.AddTextParameter("Bad Text", TextKindEnum.Line);
            builder.AddProperty("Bad Property", PropertyKindEnum.String, "Bad Text");
            builder.AddElement("Bad Element", "Bad Text", ["Bad Property"]);
            builder.AddRelation("Bad Relation", "Bad Element", "Bad Element", "Bad Text", "Bad Text");
            builder.AddContext("Bad Context", "Bad Text", ["Bad Element"]);
        });

        Assert.Equal(5, exception.ErrorList.Count);
        Assert.All(exception.ErrorList, x => Assert.Contains("the identifier is not valid", x));
    }

    #endregion -- Identifiers --

    #region -- What is checked --

    [Theory]
    [InlineData("text parameter", "Text parameter 'person-title': defined more than once.")]
    [InlineData("property", "Property 'name': defined more than once.")]
    [InlineData("element", "Element 'person': defined more than once.")]
    [InlineData("relation", "Relation 'person-involved-in-case': defined more than once.")]
    [InlineData("context", "Context 'case-file': defined more than once.")]
    public void ADefinition_AddedTwice_IsReported(string kind, string expected)
    {
        DefinitionException exception = Fails(WithExtra(builder =>
        {
            switch (kind)
            {
                case "text parameter": builder.AddTextParameter("person-title", TextKindEnum.Line); break;
                case "property": builder.AddProperty("name", PropertyKindEnum.String, "name-title"); break;
                case "element": builder.AddElement("person", "person-title", ["name"]); break;
                case "relation": builder.AddRelation("person-involved-in-case", "person", "case", "involved-in-title", "involved-persons-title"); break;
                case "context": builder.AddContext("case-file", "case-file-title", ["person"]); break;
            }
        }));

        Assert.Equal([expected], exception.ErrorList);
    }

    [Fact]
    public void ATextThatIsNotDefined_IsReported_ForPropertyElementRelationAndContext()
    {
        DefinitionException exception = Fails(builder =>
        {
            builder.AddTextParameter("ok-title", TextKindEnum.Line);
            builder.AddProperty("prop", PropertyKindEnum.String, "missing-a");
            builder.AddElement("elem", "missing-b", []);
            builder.AddRelation("rel", "elem", "elem", "missing-c", "missing-d");
            builder.AddContext("ctx", "missing-e", ["elem"]);
        });

        Assert.Equal(
        [
            "Property 'prop': the title text parameter 'missing-a' is not defined.",
            "Element 'elem': the title text parameter 'missing-b' is not defined.",
            "Relation 'rel': the forward title text parameter 'missing-c' is not defined.",
            "Relation 'rel': the inverse title text parameter 'missing-d' is not defined.",
            "Context 'ctx': the title text parameter 'missing-e' is not defined."
        ], exception.ErrorList);
    }

    [Fact]
    public void ADescriptionIsOptional_ButMustExistWhenGiven()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(WithExtra(builder => { builder.AddProperty("described", PropertyKindEnum.String, "name-title", "role-title"); })));
        Assert.Equal("role-title", registry.GetProperty("described").DescriptionText);

        DefinitionException exception = Fails(WithExtra(builder => builder.AddProperty("described", PropertyKindEnum.String, "name-title", "missing")));
        Assert.Equal(["Property 'described': the description text parameter 'missing' is not defined."], exception.ErrorList);
    }

    [Fact]
    public void ATitleIsRequired()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddProperty("untitled", PropertyKindEnum.String, string.Empty)));

        Assert.Equal(["Property 'untitled': a title text parameter is required."], exception.ErrorList);
    }

    [Fact]
    public void AnElement_ThatUsesAPropertyThatIsNotDefined_IsReported()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddElement("vehicle", "person-title", ["name", "plate"])));

        Assert.Equal(["Element 'vehicle': property 'plate' is not defined."], exception.ErrorList);
    }

    [Fact]
    public void AnElement_ThatListsAPropertyTwice_IsReported()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddElement("vehicle", "person-title", ["name", "name"])));

        Assert.Equal(["Element 'vehicle': property 'name' is listed more than once."], exception.ErrorList);
    }

    [Fact]
    public void ARelation_BetweenElementsThatAreNotDefined_IsReported()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddRelation("owns", "person", "vehicle", "involved-in-title", "involved-persons-title")));
        Assert.Equal(["Relation 'owns': element (to) 'vehicle' is not defined."], exception.ErrorList);

        exception = Fails(WithExtra(builder => builder.AddRelation("owns", "vehicle", "person", "involved-in-title", "involved-persons-title")));
        Assert.Equal(["Relation 'owns': element (from) 'vehicle' is not defined."], exception.ErrorList);
    }

    [Fact]
    public void ARelation_WithAnAttributeThatIsNotDefinedOrListedTwice_IsReported()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddRelation("owns", "person", "case", "involved-in-title", "involved-persons-title", attributes: ["since", "role", "role"])));

        Assert.Equal(
        [
            "Relation 'owns': attribute property 'since' is not defined.",
            "Relation 'owns': attribute property 'role' is listed more than once."
        ], exception.ErrorList);
    }

    [Fact]
    public void AContext_NeedsElements_ThatAreDefinedAndListedOnce()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddContext("nobody", "case-file-title", [])));
        Assert.Equal(["Context 'nobody': a context needs at least one element."], exception.ErrorList);

        exception = Fails(WithExtra(builder => builder.AddContext("extra", "case-file-title", ["person", "vehicle", "person"])));
        Assert.Equal(
        [
            "Context 'extra': element 'vehicle' is not defined.",
            "Context 'extra': element 'person' is listed more than once."
        ], exception.ErrorList);
    }

    [Theory]
    [InlineData(PropertyKindEnum.Boolean)]
    [InlineData(PropertyKindEnum.DateTime)]
    [InlineData(PropertyKindEnum.Decimal)]
    [InlineData(PropertyKindEnum.Image)]
    [InlineData(PropertyKindEnum.Integer)]
    public void AMaximumLength_OnAPropertyThatIsNotAString_IsReported(PropertyKindEnum kind)
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddProperty("sized", kind, "name-title", maxLength: 10)));

        Assert.Equal([$"Property 'sized': a maximum length only applies to a String property, not {kind}."], exception.ErrorList);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AMaximumLength_OfLessThanOne_IsReported(int maxLength)
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddProperty("sized", PropertyKindEnum.String, "name-title", maxLength: maxLength)));

        Assert.Equal(["Property 'sized': the maximum length must be at least 1."], exception.ErrorList);
    }

    [Fact]
    public void EveryProblemIsReportedTogether_NotOnlyTheFirst()
    {
        DefinitionException exception = Fails(WithExtra(builder =>
        {
            builder.AddProperty("a", PropertyKindEnum.Integer, "missing", maxLength: 5);
            builder.AddElement("b", "person-title", ["nothing"]);
            builder.AddContext("c", "case-file-title", []);
        }));

        Assert.Equal(4, exception.ErrorList.Count);
        Assert.Contains("the title text parameter 'missing' is not defined", exception.Message);
        Assert.Contains("4 problems", exception.Message);
        Assert.All(exception.ErrorList, x => Assert.Contains(x, exception.Message));
    }

    [Fact]
    public void TheMessage_SaysProblemInTheSingular_ForOneProblem()
    {
        DefinitionException exception = Fails(WithExtra(builder => builder.AddContext("nobody", "case-file-title", [])));

        Assert.Contains("(1 problem)", exception.Message);
    }

    #endregion -- What is checked --

    #region -- The registry is read-only --

    [Fact]
    public void TheListsOfTheRegistry_CannotBeChanged()
    {
        DefinitionRegistry registry = DefinitionBuilder.Create(new ActionDefinition(DefineValid));

        Assert.IsNotType<List<DefinitionElement>>(registry.ElementList);
        Assert.Throws<NotSupportedException>(() => ((IList<DefinitionElement>)registry.ElementList).Add(registry.ElementList[0]));
    }

    [Fact]
    public void ADefinition_BuiltOnce_IsNotChangedByLaterCallsOnTheBuilder()
    {
        DefinitionBuilder builder = new();
        builder.Add(new ActionDefinition(DefineValid));
        DefinitionRegistry first = builder.Build();

        builder.AddTextParameter("late-title", TextKindEnum.Line);
        DefinitionRegistry second = builder.Build();

        Assert.Equal(7, first.TextParameterList.Count);
        Assert.Equal(8, second.TextParameterList.Count);
    }

    #endregion -- The registry is read-only --
}

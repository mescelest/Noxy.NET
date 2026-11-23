// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the service calls are left on their default token.

#pragma warning disable xUnit1051

using Noxy.NET.EntityManagement.Application;
using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Persistence.Repositories;
using Noxy.NET.Test.EntityManagement.Definitions;
using Noxy.NET.Test.EntityManagement.Persistence;

namespace Noxy.NET.Test.EntityManagement.Application;

public class ContextServiceTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private const string Involved = "person-involved-in-case";
    private const string LedBy = "case-led-by-person";
    private const string HasVehicle = "person-has-vehicle";
    private const string HasWeapon = "person-has-weapon";
    private const string MarriedTo = "person-married-to-person";

    private const string CaseFile = "case-file";
    private const string People = "people";

    private readonly TestDatabase _db = new();
    private readonly ManualTimeProvider _time = new(T0);
    private readonly DefinitionRegistry _definition;

    public ContextServiceTest()
    {
        _definition = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddTextParameter("title", TextKindEnum.Line);

            builder.AddProperty("name", PropertyKindEnum.String, "title", isRequired: true);
            builder.AddProperty("role", PropertyKindEnum.String, "title", isRequired: true);
            builder.AddProperty("note", PropertyKindEnum.String, "title");

            builder.AddElement("person", "title", ["name"]);
            builder.AddElement("case", "title", ["name"]);
            builder.AddElement("vehicle", "title", ["name"]);
            builder.AddElement("weapon", "title", ["name"]);

            builder.AddRelation(Involved, "person", "case", "title", "title", RelationCardinalityEnum.ManyToMany, ["role", "note"]);
            builder.AddRelation(LedBy, "case", "person", "title", "title", RelationCardinalityEnum.ManyToOne);
            builder.AddRelation(HasVehicle, "person", "vehicle", "title", "title", RelationCardinalityEnum.OneToMany);
            builder.AddRelation(HasWeapon, "person", "weapon", "title", "title", RelationCardinalityEnum.OneToMany);
            builder.AddRelation(MarriedTo, "person", "person", "title", "title", RelationCardinalityEnum.OneToOne);

            // The weapon is not in the case file, so the relation to it is never part of it.
            builder.AddContext(CaseFile, "title", ["case", "person", "vehicle"]);
            builder.AddContext(People, "title", ["person"]);
        }));
    }

    public void Dispose() => _db.Dispose();

    // The services on new contexts each time, so what they read is what is in the database.
    private ElementService Elements() => new(_definition, new DataRepository(_db.CreateContext(), _time), _time);

    private RelationService Relations() => new(_definition, new DataRepository(_db.CreateContext(), _time), new RelationRepository(_db.CreateContext(), _time), _time);

    private ContextService Contexts() => new(_definition, Elements(), new DataRepository(_db.CreateContext(), _time), new RelationRepository(_db.CreateContext(), _time));

    private static Dictionary<string, object?> Values(params (string Identifier, object? Value)[] list) => list.ToDictionary(x => x.Identifier, x => x.Value);

    private async Task<Guid> Element(string elementIdentifier, string name)
    {
        return (await Elements().CreateElement(elementIdentifier, Values(("name", name)), UserA)).ID;
    }

    private async Task<Guid> Link(string relationIdentifier, Guid from, Guid to, params (string Identifier, object? Value)[] attributeList)
    {
        return (await Relations().CreateRelation(relationIdentifier, from, to, Values(attributeList), UserA)).ID;
    }

    private static ContextRelationGroupModel Group(ContextModel context, string relationIdentifier, RelationDirectionEnum direction)
    {
        return Assert.Single(context.RelationGroupList, x => x.RelationIdentifier == relationIdentifier && x.Direction == direction);
    }

    private static List<string> Names(ContextRelationGroupModel group) => group.RelatedList.Select(x => (string)x.Element.PropertyMap["name"].Value).ToList();

    #region -- Starting from an element --

    [Fact]
    public async Task StartingFromACase_GivesTheCaseWithItsValues_AndThePeopleInvolvedWithTheirRoles()
    {
        Guid caseID = await Element("case", "Case 1");
        Guid ann = await Element("person", "Ann");
        Guid bob = await Element("person", "Bob");
        Guid annLink = await Link(Involved, ann, caseID, ("role", "Witness"), ("note", "Saw it"));
        await Link(Involved, bob, caseID, ("role", "Suspect"));

        ContextModel? context = await Contexts().GetContext(CaseFile, caseID);

        Assert.NotNull(context);
        Assert.Equal(CaseFile, context.ContextIdentifier);
        Assert.Equal(("case", "Case 1"), (context.Element.ElementIdentifier, context.Element.PropertyMap["name"].Value));

        ContextRelationGroupModel group = Group(context, Involved, RelationDirectionEnum.Inverse);
        Assert.Equal("person", group.ElementIdentifier);
        Assert.Equal(["Ann", "Bob"], Names(group));

        ContextRelatedModel related = group.RelatedList[0];
        Assert.Equal(annLink, related.RelationID);
        Assert.Equal("Witness", related.AttributeMap["role"].Value);
        Assert.Equal("Saw it", related.AttributeMap["note"].Value);
        Assert.Equal(ann, related.Element.ID);
    }

    [Fact]
    public async Task StartingFromAPerson_GivesTheCasesTheyAreIn_AndTheVehiclesTheyHave_ButNotTheOtherPeopleInTheCases()
    {
        Guid ann = await Element("person", "Ann");
        Guid bob = await Element("person", "Bob");
        Guid caseID = await Element("case", "Case 1");
        Guid car = await Element("vehicle", "Car");
        await Link(Involved, ann, caseID, ("role", "Witness"));
        await Link(Involved, bob, caseID, ("role", "Suspect"));
        await Link(HasVehicle, ann, car);

        ContextModel? context = await Contexts().GetContext(CaseFile, ann);

        Assert.NotNull(context);
        Assert.Equal(["Case 1"], Names(Group(context, Involved, RelationDirectionEnum.Forward)));
        Assert.Equal(["Car"], Names(Group(context, HasVehicle, RelationDirectionEnum.Forward)));
        Assert.DoesNotContain(context.RelationGroupList.SelectMany(x => x.RelatedList), x => x.Element.ID == bob);
    }

    [Fact]
    public async Task TwoRelationsBetweenTheSameKinds_AreTwoGroups_EachWithItsOwnRelationAndDirection()
    {
        Guid ann = await Element("person", "Ann");
        Guid bob = await Element("person", "Bob");
        Guid caseID = await Element("case", "Case 1");
        await Link(Involved, ann, caseID, ("role", "Witness"));
        await Link(LedBy, caseID, bob);

        ContextModel? context = await Contexts().GetContext(CaseFile, caseID);

        Assert.NotNull(context);
        Assert.Equal(["Ann"], Names(Group(context, Involved, RelationDirectionEnum.Inverse)));
        Assert.Equal(["Bob"], Names(Group(context, LedBy, RelationDirectionEnum.Forward)));
    }

    [Fact]
    public async Task ARelationBetweenTwoElementsOfTheSameKind_IsReadBothWays()
    {
        Guid ann = await Element("person", "Ann");
        Guid bob = await Element("person", "Bob");
        await Link(MarriedTo, ann, bob);

        ContextModel? ofAnn = await Contexts().GetContext(People, ann);
        ContextModel? ofBob = await Contexts().GetContext(People, bob);

        Assert.NotNull(ofAnn);
        Assert.NotNull(ofBob);
        Assert.Equal(["Bob"], Names(Group(ofAnn, MarriedTo, RelationDirectionEnum.Forward)));
        Assert.Empty(Group(ofAnn, MarriedTo, RelationDirectionEnum.Inverse).RelatedList);
        Assert.Equal(["Ann"], Names(Group(ofBob, MarriedTo, RelationDirectionEnum.Inverse)));
        Assert.Empty(Group(ofBob, MarriedTo, RelationDirectionEnum.Forward).RelatedList);
    }

    [Fact]
    public async Task OnlyTheKindsOfTheContext_AreIncluded()
    {
        Guid ann = await Element("person", "Ann");
        Guid gun = await Element("weapon", "Gun");
        Guid car = await Element("vehicle", "Car");
        await Link(HasWeapon, ann, gun);
        await Link(HasVehicle, ann, car);

        ContextModel? caseFile = await Contexts().GetContext(CaseFile, ann);
        ContextModel? people = await Contexts().GetContext(People, ann);

        Assert.NotNull(caseFile);
        Assert.NotNull(people);
        Assert.DoesNotContain(caseFile.RelationGroupList, x => x.RelationIdentifier == HasWeapon);
        Assert.Single(caseFile.RelationGroupList, x => x.RelationIdentifier == HasVehicle);
        Assert.DoesNotContain(people.RelationGroupList, x => x.RelationIdentifier is HasWeapon or HasVehicle);
    }

    [Fact]
    public async Task AGroupIsThereWhenTheDefinitionAllowsIt_AlsoWhenNothingIsRelatedYet()
    {
        Guid ann = await Element("person", "Ann");

        ContextModel? context = await Contexts().GetContext(CaseFile, ann);

        Assert.NotNull(context);
        Assert.Equal(
            [(Involved, RelationDirectionEnum.Forward), (LedBy, RelationDirectionEnum.Inverse), (HasVehicle, RelationDirectionEnum.Forward), (MarriedTo, RelationDirectionEnum.Forward), (MarriedTo, RelationDirectionEnum.Inverse)],
            context.RelationGroupList.Select(x => (x.RelationIdentifier, x.Direction)).ToList());
        Assert.All(context.RelationGroupList, x => Assert.Empty(x.RelatedList));
    }

    [Fact]
    public async Task ALinkThatHasEnded_IsNotInTheContext_AndNeitherIsAnElementThatHasEnded()
    {
        Guid caseID = await Element("case", "Case 1");
        Guid ann = await Element("person", "Ann");
        Guid bob = await Element("person", "Bob");
        Guid cat = await Element("person", "Cat");
        Guid annLink = await Link(Involved, ann, caseID, ("role", "Witness"));
        await Link(Involved, bob, caseID, ("role", "Suspect"));
        await Link(Involved, cat, caseID, ("role", "Victim"));

        await Relations().EndRelation(annLink, UserA);
        await Elements().EndElement(cat, UserA);

        ContextModel? context = await Contexts().GetContext(CaseFile, caseID);

        Assert.NotNull(context);
        Assert.Equal(["Bob"], Names(Group(context, Involved, RelationDirectionEnum.Inverse)));
    }

    [Fact]
    public async Task TheRelatedElements_AreInTheOrderTheyWereLinked()
    {
        Guid caseID = await Element("case", "Case 1");
        Guid ann = await Element("person", "Ann");
        Guid bob = await Element("person", "Bob");
        Guid cat = await Element("person", "Cat");

        await Link(Involved, cat, caseID, ("role", "r"));
        _time.Now = _time.Now.AddMinutes(1);
        await Link(Involved, ann, caseID, ("role", "r"));
        _time.Now = _time.Now.AddMinutes(1);
        await Link(Involved, bob, caseID, ("role", "r"));

        ContextModel? context = await Contexts().GetContext(CaseFile, caseID);

        Assert.NotNull(context);
        Assert.Equal(["Cat", "Ann", "Bob"], Names(Group(context, Involved, RelationDirectionEnum.Inverse)));
    }

    #endregion

    #region -- What cannot be asked --

    [Fact]
    public async Task AnElementThatIsNotThere_OrHasEnded_GivesNull()
    {
        Guid ann = await Element("person", "Ann");
        await Elements().EndElement(ann, UserA);

        Assert.Null(await Contexts().GetContext(CaseFile, Guid.NewGuid()));
        Assert.Null(await Contexts().GetContext(CaseFile, ann));
    }

    [Fact]
    public async Task AnElementOfAKindOutsideTheContext_IsABadRequest()
    {
        Guid gun = await Element("weapon", "Gun");

        DataValidationException exception = await Assert.ThrowsAsync<DataValidationException>(() => Contexts().GetContext(CaseFile, gun));

        Assert.Equal(DataErrorEnum.WrongElement, Assert.Single(exception.ErrorList).Code);
    }

    [Fact]
    public async Task AContextThatIsNotDefined_IsAMistakeOfTheCode()
    {
        Guid ann = await Element("person", "Ann");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => Contexts().GetContext("nope", ann));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Contexts().GetElementPage("nope", "person", 0, 10));
    }

    #endregion

    #region -- Without a starting element --

    [Fact]
    public async Task ThePageOfAKindInTheContext_IsTheSameAsThePageOfTheKind()
    {
        for (int i = 0; i < 5; i++)
        {
            await Element("person", $"P{i}");
            _time.Now = _time.Now.AddMinutes(1);
        }

        ElementPageModel page = await Contexts().GetElementPage(CaseFile, "person", 1, 2);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(["P1", "P2"], page.ElementList.Select(x => (string)x.PropertyMap["name"].Value).ToList());
    }

    [Fact]
    public async Task ThePageOfAKindOutsideTheContext_IsABadRequest()
    {
        DataValidationException exception = await Assert.ThrowsAsync<DataValidationException>(() => Contexts().GetElementPage(CaseFile, "weapon", 0, 10));

        Assert.Equal(DataErrorEnum.WrongElement, Assert.Single(exception.ErrorList).Code);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Contexts().GetElementPage(CaseFile, "nope", 0, 10));
    }

    #endregion
}

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

public class RelationServiceTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private const string Involved = "person-involved-in-case";
    private const string LedBy = "case-led-by-person";
    private const string HasVehicle = "person-has-vehicle";
    private const string MarriedTo = "person-married-to-person";

    private readonly TestDatabase _db = new();
    private readonly ManualTimeProvider _time = new(T0);
    private readonly DefinitionRegistry _definition;

    public RelationServiceTest()
    {
        _definition = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddTextParameter("title", TextKindEnum.Line);

            builder.AddProperty("name", PropertyKindEnum.String, "title", isRequired: true);
            builder.AddProperty("role", PropertyKindEnum.String, "title", isRequired: true, maxLength: 10);
            builder.AddProperty("note", PropertyKindEnum.String, "title");
            builder.AddProperty("since", PropertyKindEnum.DateTime, "title");

            builder.AddElement("person", "title", ["name"]);
            builder.AddElement("case", "title", ["name"]);
            builder.AddElement("vehicle", "title", ["name"]);

            // Many to many, with attributes.
            builder.AddRelation(Involved, "person", "case", "title", "title", RelationCardinalityEnum.ManyToMany, ["role", "note", "since"]);

            // A case has one lead, and a person can lead many cases.
            builder.AddRelation(LedBy, "case", "person", "title", "title", RelationCardinalityEnum.ManyToOne);

            // A person has many vehicles, and a vehicle has one owner.
            builder.AddRelation(HasVehicle, "person", "vehicle", "title", "title", RelationCardinalityEnum.OneToMany);

            // A person has one partner.
            builder.AddRelation(MarriedTo, "person", "person", "title", "title", RelationCardinalityEnum.OneToOne);
        }));
    }

    public void Dispose() => _db.Dispose();

    // The services on new contexts each time, so what they read is what is in the database.
    private ElementService Elements() => new(_definition, new DataRepository(_db.CreateContext(), _time), _time);

    private RelationService Relations() => new(_definition, new DataRepository(_db.CreateContext(), _time), new RelationRepository(_db.CreateContext(), _time), _time);

    private static Dictionary<string, object?> Values(params (string Identifier, object? Value)[] list) => list.ToDictionary(x => x.Identifier, x => x.Value);

    private async Task<Guid> Element(string elementIdentifier, string name = "x")
    {
        return (await Elements().CreateElement(elementIdentifier, Values(("name", name)), UserA)).ID;
    }

    private static DataValidationException Errors(Func<Task> act)
    {
        return Assert.ThrowsAsync<DataValidationException>(act).GetAwaiter().GetResult();
    }

    #region -- Making a relation --

    [Fact]
    public async Task ARelation_IsMadeWithItsAttributes_AndComesBackInTheirTypes()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        DateTime since = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);

        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness"), ("note", "Saw it"), ("since", since)), UserA);

        RelationModel? found = await Relations().FindRelation(created.ID);
        Assert.NotNull(found);
        Assert.Equal((Involved, person, caseElement, UserA, T0), (found.RelationIdentifier, found.FromElementID, found.ToElementID, found.CreatedByUserID, found.TimeCreated));
        Assert.Equal("Witness", found.AttributeMap["role"].Value);
        Assert.Equal("Saw it", found.AttributeMap["note"].Value);
        Assert.Equal(since, found.AttributeMap["since"].Value);
        Assert.Equal(PropertyKindEnum.DateTime, found.AttributeMap["since"].Kind);
    }

    [Fact]
    public async Task TheReturnedModel_IsTheSameAsWhatIsStored()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");

        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA);
        RelationModel stored = (await Relations().FindRelation(created.ID))!;

        Assert.Equal((created.ID, created.RelationIdentifier, created.FromElementID, created.ToElementID), (stored.ID, stored.RelationIdentifier, stored.FromElementID, stored.ToElementID));
        Assert.All(created.AttributeMap, x => Assert.Equal(x.Value, stored.AttributeMap[x.Key]));
    }

    [Fact]
    public async Task ARelationWithoutAttributes_NeedsNone()
    {
        Guid caseElement = await Element("case");
        Guid person = await Element("person");

        RelationModel created = await Relations().CreateRelation(LedBy, caseElement, person, Values(), UserA);

        Assert.Empty(created.AttributeMap);
    }

    [Fact]
    public async Task ARequiredAttribute_MustHaveAValue_AndAnEmptyOptionalOneIsLeftOut()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");

        DataValidationException exception = Errors(() => Relations().CreateRelation(Involved, person, caseElement, Values(("note", "Saw it")), UserA));
        Assert.Equal([new DataError("role", DataErrorEnum.Required, "The property 'role' is required.")], exception.ErrorList);

        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness"), ("note", ""), ("since", null)), UserA);
        Assert.Equal(["role"], created.AttributeMap.Keys);
    }

    [Fact]
    public async Task AnAttributeTheRelationDoesNotHave_OrOfTheWrongType_OrTooLong_IsReported()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");

        // "name" is a property of the elements, not an attribute of the relation.
        DataValidationException exception = Errors(() => Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness"), ("name", "x"), ("since", "yesterday")), UserA));
        Assert.Equal([("name", DataErrorEnum.UnknownProperty), ("since", DataErrorEnum.WrongType)], exception.ErrorList.OrderBy(x => x.PropertyIdentifier).Select(x => (x.PropertyIdentifier!, x.Code)));

        exception = Errors(() => Relations().CreateRelation(Involved, person, caseElement, Values(("role", "A role that is too long")), UserA));
        Assert.Equal(DataErrorEnum.TooLong, Assert.Single(exception.ErrorList).Code);

        Assert.Empty(await Relations().GetRelationList(person));
    }

    [Fact]
    public async Task AnElementThatDoesNotExist_OrHasEnded_IsReported_WhichEndItIs()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        Guid ended = await Element("case");
        await Elements().EndElement(ended, UserA);

        DataValidationException toNothing = Errors(() => Relations().CreateRelation(Involved, person, Guid.NewGuid(), Values(("role", "Witness")), UserA));
        DataValidationException fromNothing = Errors(() => Relations().CreateRelation(Involved, Guid.NewGuid(), caseElement, Values(("role", "Witness")), UserA));
        DataValidationException toEnded = Errors(() => Relations().CreateRelation(Involved, person, ended, Values(("role", "Witness")), UserA));
        DataValidationException both = Errors(() => Relations().CreateRelation(Involved, Guid.NewGuid(), Guid.NewGuid(), Values(("role", "Witness")), UserA));

        Assert.Contains("reads to", Assert.Single(toNothing.ErrorList).Message);
        Assert.Contains("reads from", Assert.Single(fromNothing.ErrorList).Message);
        Assert.All(new[] { toNothing, fromNothing, toEnded }, x => Assert.Equal(DataErrorEnum.ElementNotFound, Assert.Single(x.ErrorList).Code));
        Assert.Equal(2, both.ErrorList.Count);
    }

    [Fact]
    public async Task AnElementOfTheWrongKind_IsReported()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        Guid vehicle = await Element("vehicle");

        // The right elements the wrong way round, and a vehicle where a case is wanted.
        DataValidationException swapped = Errors(() => Relations().CreateRelation(Involved, caseElement, person, Values(("role", "Witness")), UserA));
        DataValidationException wrong = Errors(() => Relations().CreateRelation(Involved, person, vehicle, Values(("role", "Witness")), UserA));

        Assert.Equal(2, swapped.ErrorList.Count);
        Assert.All(swapped.ErrorList, x => Assert.Equal(DataErrorEnum.WrongElement, x.Code));
        Assert.Equal(DataErrorEnum.WrongElement, Assert.Single(wrong.ErrorList).Code);
        Assert.Contains("'case'", wrong.ErrorList[0].Message);
    }

    [Fact]
    public async Task EveryProblemIsReportedTogether_AndNothingIsStored()
    {
        Guid person = await Element("person");
        Guid vehicle = await Element("vehicle");

        DataValidationException exception = Errors(() => Relations().CreateRelation(Involved, person, vehicle, Values(("name", "x"), ("since", 5)), UserA));

        // The vehicle is the wrong kind, there is no attribute "name", the "since" is the wrong type, and the "role" is missing.
        Assert.Equal(4, exception.ErrorList.Count);
        Assert.Empty(await Relations().GetRelationList(person));
        Assert.Empty(await Relations().GetRelationList(vehicle));
    }

    [Fact]
    public async Task TheSameRelationTwice_IsReported_ButAnotherKindIsNot()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA);

        DataValidationException exception = Errors(() => Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Suspect")), UserA));

        Assert.Equal(DataErrorEnum.AlreadyRelated, Assert.Single(exception.ErrorList).Code);
        await Relations().CreateRelation(LedBy, caseElement, person, Values(), UserA);
        Assert.Equal(2, (await Relations().GetRelationList(person)).Count);
    }

    [Fact]
    public async Task ARelationThatIsNotDefined_IsAMistakeOfTheCode()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => Relations().CreateRelation("nothing", person, caseElement, Values(), UserA));
    }

    #endregion -- Making a relation --

    #region -- Cardinality --

    [Fact]
    public async Task ManyToMany_HasNoLimit()
    {
        Guid person1 = await Element("person");
        Guid person2 = await Element("person");
        Guid case1 = await Element("case");
        Guid case2 = await Element("case");

        await Relations().CreateRelation(Involved, person1, case1, Values(("role", "a")), UserA);
        await Relations().CreateRelation(Involved, person1, case2, Values(("role", "b")), UserA);
        await Relations().CreateRelation(Involved, person2, case1, Values(("role", "c")), UserA);
        await Relations().CreateRelation(Involved, person2, case2, Values(("role", "d")), UserA);

        Assert.Equal(2, (await Relations().GetRelationList(person1)).Count);
        Assert.Equal(2, (await Relations().GetRelationList(case1)).Count);
    }

    [Fact]
    public async Task ManyToOne_GivesTheElementItReadsFromOnlyOne_ButAnElementItReadsToCanHaveMany()
    {
        Guid case1 = await Element("case");
        Guid case2 = await Element("case");
        Guid lead1 = await Element("person");
        Guid lead2 = await Element("person");

        await Relations().CreateRelation(LedBy, case1, lead1, Values(), UserA);
        await Relations().CreateRelation(LedBy, case2, lead1, Values(), UserA);

        DataValidationException exception = Errors(() => Relations().CreateRelation(LedBy, case1, lead2, Values(), UserA));
        Assert.Equal(DataErrorEnum.RelationLimitReached, Assert.Single(exception.ErrorList).Code);
        Assert.Equal(2, (await Relations().GetRelationList(lead1)).Count);
    }

    [Fact]
    public async Task OneToMany_GivesTheElementItReadsToOnlyOne_ButAnElementItReadsFromCanHaveMany()
    {
        Guid owner1 = await Element("person");
        Guid owner2 = await Element("person");
        Guid vehicle1 = await Element("vehicle");
        Guid vehicle2 = await Element("vehicle");

        await Relations().CreateRelation(HasVehicle, owner1, vehicle1, Values(), UserA);
        await Relations().CreateRelation(HasVehicle, owner1, vehicle2, Values(), UserA);

        DataValidationException exception = Errors(() => Relations().CreateRelation(HasVehicle, owner2, vehicle1, Values(), UserA));
        Assert.Equal(DataErrorEnum.RelationLimitReached, Assert.Single(exception.ErrorList).Code);
    }

    [Fact]
    public async Task OneToOne_LimitsBothEnds()
    {
        Guid a = await Element("person");
        Guid b = await Element("person");
        Guid c = await Element("person");
        Guid d = await Element("person");
        await Relations().CreateRelation(MarriedTo, a, b, Values(), UserA);

        Assert.Equal(DataErrorEnum.RelationLimitReached, Assert.Single(Errors(() => Relations().CreateRelation(MarriedTo, a, c, Values(), UserA)).ErrorList).Code);
        Assert.Equal(DataErrorEnum.RelationLimitReached, Assert.Single(Errors(() => Relations().CreateRelation(MarriedTo, d, b, Values(), UserA)).ErrorList).Code);

        await Relations().CreateRelation(MarriedTo, c, d, Values(), UserA);
    }

    [Fact]
    public async Task AnEndedRelation_FreesTheLimit()
    {
        Guid case1 = await Element("case");
        Guid lead1 = await Element("person");
        Guid lead2 = await Element("person");
        RelationModel first = await Relations().CreateRelation(LedBy, case1, lead1, Values(), UserA);

        await Relations().EndRelation(first.ID, UserA);

        Assert.NotNull(await Relations().CreateRelation(LedBy, case1, lead2, Values(), UserA));
    }

    #endregion -- Cardinality --

    #region -- Reading --

    [Fact]
    public async Task TheRelationsOfAnElement_AreListedFromBothEnds_WithTheirAttributes()
    {
        Guid person = await Element("person");
        Guid case1 = await Element("case");
        Guid case2 = await Element("case");

        _time.Now = T0.AddMinutes(1);
        RelationModel involved = await Relations().CreateRelation(Involved, person, case1, Values(("role", "Witness")), UserA);
        _time.Now = T0.AddMinutes(2);
        RelationModel led = await Relations().CreateRelation(LedBy, case2, person, Values(), UserA);

        // From the person: the link it reads from, and the one it reads to. From a case: the same links, the other way.
        IReadOnlyList<RelationModel> ofPerson = await Relations().GetRelationList(person);
        Assert.Equal([involved.ID, led.ID], ofPerson.Select(x => x.ID));
        Assert.Equal("Witness", ofPerson[0].AttributeMap["role"].Value);
        Assert.Equal([(person, case1), (case2, person)], ofPerson.Select(x => (x.FromElementID, x.ToElementID)));

        Assert.Equal([involved.ID], (await Relations().GetRelationList(case1)).Select(x => x.ID));
        Assert.Equal([led.ID], (await Relations().GetRelationList(case2)).Select(x => x.ID));
        Assert.Equal([involved.ID], (await Relations().GetRelationList(person, Involved)).Select(x => x.ID));
    }

    [Fact]
    public async Task AnElementWithoutRelations_HasNone_AndARelationThatIsNotThere_IsNotFound()
    {
        Assert.Empty(await Relations().GetRelationList(Guid.NewGuid()));
        Assert.Null(await Relations().FindRelation(Guid.NewGuid()));
    }

    #endregion -- Reading --

    #region -- Changing a relation --

    [Fact]
    public async Task TheAttributesOfARelation_AreChanged_AndTheOldOnesGoToTheHistory()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness"), ("note", "Saw it")), UserA);
        _time.Now = T0.AddHours(1);

        RelationModel? changed = await Relations().ChangeRelation(created.ID, Values(("role", "Suspect"), ("note", null), ("since", T0)), UserB);

        Assert.NotNull(changed);
        Assert.Equal("Suspect", changed.AttributeMap["role"].Value);
        Assert.DoesNotContain("note", changed.AttributeMap.Keys);
        Assert.Equal(T0, changed.AttributeMap["since"].Value);
        Assert.Equal((UserB, T0.AddHours(1)), (changed.AttributeMap["role"].CreatedByUserID, changed.AttributeMap["role"].TimeCreated));

        IReadOnlyList<PropertyHistoryModel> history = await Relations().GetAttributeHistory(created.ID);
        Assert.Equal(["note", "role"], history.Select(x => x.PropertyIdentifier).Order());
        Assert.Equal(("Witness", UserA, UserB, T0.AddHours(1)),
            (history.Single(x => x.PropertyIdentifier == "role").Value, history.Single(x => x.PropertyIdentifier == "role").CreatedByUserID, history.Single(x => x.PropertyIdentifier == "role").EndedByUserID,
                history.Single(x => x.PropertyIdentifier == "role").TimeEnded));
    }

    [Fact]
    public async Task ChangingAnAttributeToTheSameValue_IsNotAChange()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA);
        _time.Now = T0.AddHours(1);

        RelationModel? same = await Relations().ChangeRelation(created.ID, Values(("role", "Witness")), UserB);

        Assert.NotNull(same);
        Assert.Equal(T0, same.AttributeMap["role"].TimeCreated);
        Assert.Empty(await Relations().GetAttributeHistory(created.ID));
    }

    [Fact]
    public async Task ARequiredAttribute_CannotBeCleared_AndABadValueStopsTheWholeChange()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness"), ("note", "Saw it")), UserA);

        Assert.Equal(DataErrorEnum.Required, Assert.Single(Errors(() => Relations().ChangeRelation(created.ID, Values(("role", null)), UserB)).ErrorList).Code);

        DataValidationException exception = Errors(() => Relations().ChangeRelation(created.ID, Values(("note", "Changed"), ("since", "yesterday"), ("nothing", 1)), UserB));
        Assert.Equal(2, exception.ErrorList.Count);

        RelationModel stored = (await Relations().FindRelation(created.ID))!;
        Assert.Equal(("Witness", "Saw it"), (stored.AttributeMap["role"].Value, stored.AttributeMap["note"].Value));
        Assert.Empty(await Relations().GetAttributeHistory(created.ID));
    }

    [Fact]
    public async Task ChangingARelationThatIsNotThere_OrHasEnded_GivesNull()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA);
        await Relations().EndRelation(created.ID, UserA);

        Assert.Null(await Relations().ChangeRelation(Guid.NewGuid(), Values(("role", "x")), UserB));
        Assert.Null(await Relations().ChangeRelation(created.ID, Values(("role", "x")), UserB));
    }

    #endregion -- Changing a relation --

    #region -- Ending and the history --

    [Fact]
    public async Task ARelationThatEnds_IsGone_ButTheElementsStay_AndItIsInTheHistoryOfBoth()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA);
        _time.Now = T0.AddDays(1);

        bool ended = await Relations().EndRelation(created.ID, UserB);

        Assert.True(ended);
        Assert.Null(await Relations().FindRelation(created.ID));
        Assert.Empty(await Relations().GetRelationList(person));
        Assert.NotNull(await Elements().FindElement(person));
        Assert.NotNull(await Elements().FindElement(caseElement));

        RelationHistoryModel old = Assert.Single(await Relations().GetRelationHistory(person));
        Assert.Equal((created.ID, Involved, person, caseElement, UserA, UserB, T0.AddDays(1)), (old.ID, old.RelationIdentifier, old.FromElementID, old.ToElementID, old.CreatedByUserID, old.EndedByUserID, old.TimeEnded));
        Assert.Equal([created.ID], (await Relations().GetRelationHistory(caseElement)).Select(x => x.ID));

        // Its attributes are still readable in the history.
        PropertyHistoryModel attribute = Assert.Single(await Relations().GetAttributeHistory(created.ID));
        Assert.Equal(("role", "Witness"), (attribute.PropertyIdentifier, attribute.Value));
    }

    [Fact]
    public async Task EndingARelationThatIsNotThere_OrIsAlreadyEnded_GivesFalse()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        RelationModel created = await Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA);

        Assert.False(await Relations().EndRelation(Guid.NewGuid(), UserA));
        Assert.True(await Relations().EndRelation(created.ID, UserA));
        Assert.False(await Relations().EndRelation(created.ID, UserA));
    }

    [Fact]
    public async Task EndingAnElement_EndsAllOfItsRelations_InBothDirections()
    {
        Guid person = await Element("person");
        Guid case1 = await Element("case");
        Guid case2 = await Element("case");
        Guid other = await Element("person");
        await Relations().CreateRelation(Involved, person, case1, Values(("role", "Witness")), UserA);
        await Relations().CreateRelation(LedBy, case2, person, Values(), UserA);
        RelationModel kept = await Relations().CreateRelation(Involved, other, case1, Values(("role", "Suspect")), UserA);
        _time.Now = T0.AddDays(1);

        await Elements().EndElement(person, UserB);

        Assert.Empty(await Relations().GetRelationList(person));
        Assert.Equal([kept.ID], (await Relations().GetRelationList(case1)).Select(x => x.ID));
        Assert.Empty(await Relations().GetRelationList(case2));
        Assert.Equal(2, (await Relations().GetRelationHistory(person)).Count);
        Assert.Single(await Relations().GetRelationHistory(case2));
        Assert.All(await Relations().GetRelationHistory(person), x => Assert.Equal((UserB, T0.AddDays(1)), (x.EndedByUserID, x.TimeEnded)));
    }

    [Fact]
    public async Task ARelationCannotBeMadeToAnElementThatEnded_EvenIfItWasThereAMomentAgo()
    {
        Guid person = await Element("person");
        Guid caseElement = await Element("case");
        await Elements().EndElement(caseElement, UserA);

        DataValidationException exception = Errors(() => Relations().CreateRelation(Involved, person, caseElement, Values(("role", "Witness")), UserA));

        Assert.Equal(DataErrorEnum.ElementNotFound, Assert.Single(exception.ErrorList).Code);
    }

    [Fact]
    public async Task AnElementThatNeverHadRelations_HasNoHistoryOfThem()
    {
        Assert.Empty(await Relations().GetRelationHistory(Guid.NewGuid()));
        Assert.Empty(await Relations().GetAttributeHistory(Guid.NewGuid()));
    }

    #endregion -- Ending and the history --
}

// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the repository calls are left on their default token.

#pragma warning disable xUnit1051

using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Models;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.Test.EntityManagement.Persistence;

public class RelationRepositoryTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private const string Involved = "person-involved-in-case";

    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private RelationRepository Relations(TimeProvider? time = null) => new(_db.CreateContext(), time ?? TimeProvider.System);

    private async Task<DataElement> Element(string schemaIdentifier = "person")
    {
        return await _db.CreateRepository().CreateElement(new() { SchemaIdentifier = schemaIdentifier, CreatedByUserID = UserA, TimeCreated = T0 });
    }

    private static DataRelation Link(Guid from, Guid to, string identifier = Involved, DateTime? time = null)
    {
        return new() { RelationIdentifier = identifier, FromElementID = from, ToElementID = to, CreatedByUserID = UserA, TimeCreated = time ?? T0 };
    }

    private static DataPropertyString Role(Guid owner, string value, DateTime? time = null, Guid? user = null)
    {
        return new() { OwnerID = owner, PropertyIdentifier = "role", Value = value, CreatedByUserID = user ?? UserA, TimeCreated = time ?? T0 };
    }

    private async Task<DataRelation> Created(Guid from, Guid to, string identifier = Involved, DateTime? time = null, params DataPropertyString[] attributes)
    {
        DataRelation relation = Link(from, to, identifier, time);
        CreateRelationResult result = await Relations().CreateRelation(relation,
            [.. attributes.Select(x => new DataPropertyString { OwnerID = relation.ID, PropertyIdentifier = x.PropertyIdentifier, Value = x.Value, CreatedByUserID = x.CreatedByUserID, TimeCreated = x.TimeCreated })], null, null);
        Assert.Equal(RelationCreateStatusEnum.Created, result.Status);
        return result.Relation!;
    }

    #region -- Creating a relation --

    [Fact]
    public async Task ARelation_IsCreatedWithItsAttributes_AndFound()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = Link(person.ID, caseElement.ID);

        CreateRelationResult result = await Relations().CreateRelation(relation, [Role(relation.ID, "Witness")], null, null);

        Assert.Equal(RelationCreateStatusEnum.Created, result.Status);
        DataRelation? found = await Relations().FindRelation(relation.ID);
        Assert.NotNull(found);
        Assert.Equal((Involved, person.ID, caseElement.ID, UserA, T0), (found.RelationIdentifier, found.FromElementID, found.ToElementID, found.CreatedByUserID, found.TimeCreated));

        DataPropertyString role = Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(relation.ID)));
        Assert.Equal(("role", "Witness"), (role.PropertyIdentifier, role.Value));
    }

    [Fact]
    public async Task ARelation_CanBeCreatedWithoutAttributes()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");

        DataRelation relation = await Created(person.ID, caseElement.ID);

        Assert.Empty(await _db.CreateRepository().GetPropertyList(relation.ID));
    }

    [Fact]
    public async Task ARelationBetweenAnElementThatDoesNotExist_IsNotCreated_AndSaysWhichOne()
    {
        DataElement person = await Element();

        DataRelation toNothing = Link(person.ID, Guid.NewGuid());
        DataRelation fromNothing = Link(Guid.NewGuid(), person.ID);
        DataRelation neither = Link(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(RelationCreateStatusEnum.ToElementNotFound, (await Relations().CreateRelation(toNothing, [Role(toNothing.ID, "x")], null, null)).Status);
        Assert.Equal(RelationCreateStatusEnum.FromElementNotFound, (await Relations().CreateRelation(fromNothing, [], null, null)).Status);
        Assert.Equal(RelationCreateStatusEnum.FromElementNotFound, (await Relations().CreateRelation(neither, [], null, null)).Status);

        Assert.Null(await Relations().FindRelation(toNothing.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyList(toNothing.ID));
    }

    [Fact]
    public async Task ARelationToAnElementThatHasEnded_IsNotCreated()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        await _db.CreateRepository().EndElement(caseElement.ID, UserA);

        Assert.Equal(RelationCreateStatusEnum.ToElementNotFound, (await Relations().CreateRelation(Link(person.ID, caseElement.ID), [], null, null)).Status);
    }

    [Fact]
    public async Task TheSameRelationTwice_IsNotCreated_ButAnotherKindOrDirectionIs()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        await Created(person.ID, caseElement.ID);

        Assert.Equal(RelationCreateStatusEnum.AlreadyExists, (await Relations().CreateRelation(Link(person.ID, caseElement.ID), [], null, null)).Status);

        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(person.ID, caseElement.ID, "person-owns-case"), [], null, null)).Status);
        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(caseElement.ID, person.ID), [], null, null)).Status);
    }

    [Fact]
    public async Task ARelationThatHasEnded_CanBeMadeAgain()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation first = await Created(person.ID, caseElement.ID);
        await Relations().EndRelation(first.ID, UserA);

        DataRelation second = await Created(person.ID, caseElement.ID);

        Assert.NotEqual(first.ID, second.ID);
        Assert.Single(await Relations().GetRelationList(person.ID));
        Assert.Single(await Relations().GetRelationHistoryList(person.ID));
    }

    [Fact]
    public async Task AnElementCanBeRelatedToItself()
    {
        DataElement person = await Element();

        DataRelation relation = await Created(person.ID, person.ID, "person-knows-person");

        Assert.Equal([relation.ID], (await Relations().GetRelationList(person.ID)).Select(x => x.ID));
    }

    #endregion -- Creating a relation --

    #region -- The limits --

    [Fact]
    public async Task ALimitOfOneOnTheElementItReadsFrom_AllowsOnlyOneRelationOfTheKind()
    {
        DataElement person = await Element();
        DataElement case1 = await Element("case");
        DataElement case2 = await Element("case");

        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(person.ID, case1.ID), [], 1, null)).Status);
        Assert.Equal(RelationCreateStatusEnum.FromLimitReached, (await Relations().CreateRelation(Link(person.ID, case2.ID), [], 1, null)).Status);

        Assert.Single(await Relations().GetRelationList(person.ID));
    }

    [Fact]
    public async Task ALimitOfOneOnTheElementItReadsTo_AllowsOnlyOneRelationOfTheKind()
    {
        DataElement case1 = await Element("case");
        DataElement person1 = await Element();
        DataElement person2 = await Element();

        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(person1.ID, case1.ID), [], null, 1)).Status);
        Assert.Equal(RelationCreateStatusEnum.ToLimitReached, (await Relations().CreateRelation(Link(person2.ID, case1.ID), [], null, 1)).Status);
    }

    [Fact]
    public async Task ALimitOfTwo_AllowsTwo_AndNotThree()
    {
        DataElement person = await Element();
        List<DataElement> cases = [await Element("case"), await Element("case"), await Element("case")];

        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(person.ID, cases[0].ID), [], 2, null)).Status);
        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(person.ID, cases[1].ID), [], 2, null)).Status);
        Assert.Equal(RelationCreateStatusEnum.FromLimitReached, (await Relations().CreateRelation(Link(person.ID, cases[2].ID), [], 2, null)).Status);
    }

    [Fact]
    public async Task TheLimit_OnlyCountsRelationsOfTheSameKind_AndOnlyCurrentOnes()
    {
        DataElement person = await Element();
        DataElement case1 = await Element("case");
        DataElement case2 = await Element("case");
        await Created(person.ID, case1.ID, "person-owns-case");
        DataRelation involved = await Created(person.ID, case1.ID);

        // The other kind does not count, and the one that ends does not count any more.
        Assert.Equal(RelationCreateStatusEnum.FromLimitReached, (await Relations().CreateRelation(Link(person.ID, case2.ID), [], 1, null)).Status);
        await Relations().EndRelation(involved.ID, UserA);
        Assert.Equal(RelationCreateStatusEnum.Created, (await Relations().CreateRelation(Link(person.ID, case2.ID), [], 1, null)).Status);
    }

    [Fact]
    public async Task ALimitThatIsNotAtLeastOne_IsRefused()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Relations().CreateRelation(Link(person.ID, caseElement.ID), [], 0, null));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Relations().CreateRelation(Link(person.ID, caseElement.ID), [], null, -1));
    }

    #endregion -- The limits --

    #region -- All or nothing --

    [Fact]
    public async Task AnAttributeOfAnotherOwner_OrARepeatedOne_IsRefused_AndNothingIsStored()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = Link(person.ID, caseElement.ID);

        await Assert.ThrowsAsync<ArgumentException>(() => Relations().CreateRelation(relation, [Role(Guid.NewGuid(), "x")], null, null));
        await Assert.ThrowsAsync<ArgumentException>(() => Relations().CreateRelation(relation, [Role(relation.ID, "x"), Role(relation.ID, "y")], null, null));

        Assert.Null(await Relations().FindRelation(relation.ID));
    }

    [Fact]
    public async Task WhenAnAttributeCannotBeStored_TheRelationIsNotCreatedEither()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = Link(person.ID, caseElement.ID);
        DataPropertyString role = Role(relation.ID, "Witness");

        // A row with the ID of the attribute is already in the table, so storing it fails after the relation was added.
        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(new DataPropertyString { ID = role.ID, OwnerID = Guid.NewGuid(), PropertyIdentifier = "other", Value = "x", CreatedByUserID = UserA });
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => Relations().CreateRelation(relation, [role], null, null));

        Assert.Null(await Relations().FindRelation(relation.ID));
        Assert.Empty(await Relations().GetRelationList(person.ID));
    }

    [Fact]
    public async Task TheDatabase_RefusesTwoCurrentRelationsOfAKindBetweenTwoElements()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        await using EntityManagementContext context = _db.CreateContext();
        context.Add(Link(person.ID, caseElement.ID));
        context.Add(Link(person.ID, caseElement.ID));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    #endregion -- All or nothing --

    #region -- Reading --

    [Fact]
    public async Task TheRelationsOfAnElement_AreReadFromBothEnds_OldestFirst()
    {
        DataElement person = await Element();
        DataElement case1 = await Element("case");
        DataElement case2 = await Element("case");
        DataElement other = await Element();

        DataRelation second = await Created(case2.ID, person.ID, "case-names-person", T0.AddMinutes(2));
        DataRelation first = await Created(person.ID, case1.ID, Involved, T0.AddMinutes(1));
        await Created(other.ID, case1.ID, Involved, T0);

        List<DataRelation> list = await Relations().GetRelationList(person.ID);

        // The element is the one it reads from in the first, and the one it reads to in the second.
        Assert.Equal([first.ID, second.ID], list.Select(x => x.ID));
        Assert.Equal([person.ID, case2.ID], list.Select(x => x.FromElementID));

        // The other end sees the same row: that is the inverse view.
        Assert.Equal([first.ID], (await Relations().GetRelationList(case1.ID, Involved)).Where(x => x.FromElementID == person.ID).Select(x => x.ID));
        Assert.Equal(2, (await Relations().GetRelationList(case1.ID)).Count);
    }

    [Fact]
    public async Task TheRelationsOfAnElement_CanBeFilteredByKind()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        await Created(person.ID, caseElement.ID);
        DataRelation owns = await Created(person.ID, caseElement.ID, "person-owns-case");

        Assert.Equal([owns.ID], (await Relations().GetRelationList(person.ID, "person-owns-case")).Select(x => x.ID));
        Assert.Empty(await Relations().GetRelationList(person.ID, "nothing"));
        Assert.Equal(2, (await Relations().GetRelationList(person.ID)).Count);
    }

    [Fact]
    public async Task AnElementWithoutRelations_HasNone_AndARelationThatIsNotThere_IsNotFound()
    {
        Assert.Empty(await Relations().GetRelationList(Guid.NewGuid()));
        Assert.Null(await Relations().FindRelation(Guid.NewGuid()));
    }

    #endregion -- Reading --

    #region -- Changing the attributes --

    [Fact]
    public async Task TheAttributesOfARelation_AreSetAndCleared_AndTheOldOnesMoveToTheHistory()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = await Created(person.ID, caseElement.ID, Involved, T0, Role(Guid.Empty, "Witness"));

        DataPropertyString note = new() { OwnerID = relation.ID, PropertyIdentifier = "note", Value = "Seen at 10", CreatedByUserID = UserB, TimeCreated = T0.AddHours(1) };
        bool changed = await Relations(new ManualTimeProvider(T0.AddHours(2))).ChangeRelationAttributes(relation.ID, [Role(relation.ID, "Suspect", T0.AddHours(1), UserB), note], [], UserB);

        Assert.True(changed);
        List<DataProperty> current = await _db.CreateRepository().GetPropertyList(relation.ID);
        Assert.Equal([("note", "Seen at 10"), ("role", "Suspect")], current.Cast<DataPropertyString>().Select(x => (x.PropertyIdentifier, x.Value)));

        HistoryProperty old = Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(relation.ID));
        Assert.Equal(("role", UserB, T0.AddHours(1)), (old.PropertyIdentifier, old.EndedByUserID, old.TimeEnded));

        Assert.True(await Relations(new ManualTimeProvider(T0.AddHours(3))).ChangeRelationAttributes(relation.ID, [], ["note"], UserA));
        Assert.Single(await _db.CreateRepository().GetPropertyList(relation.ID));
    }

    [Fact]
    public async Task TheAttributesOfARelationThatIsNotThere_OrHasEnded_AreNotChanged()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = await Created(person.ID, caseElement.ID);
        await Relations().EndRelation(relation.ID, UserA);

        Assert.False(await Relations().ChangeRelationAttributes(relation.ID, [Role(relation.ID, "Witness")], [], UserB));
        Assert.False(await Relations().ChangeRelationAttributes(Guid.NewGuid(), [], ["role"], UserB));

        Assert.Empty(await _db.CreateRepository().GetPropertyList(relation.ID));
    }

    #endregion -- Changing the attributes --

    #region -- Ending --

    [Fact]
    public async Task EndingARelation_MovesItAndItsAttributesToTheHistory_AndLeavesTheElements()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = await Created(person.ID, caseElement.ID, Involved, T0, Role(Guid.Empty, "Witness"));
        ManualTimeProvider time = new(T0.AddDays(1));

        bool ended = await Relations(time).EndRelation(relation.ID, UserB);

        Assert.True(ended);
        Assert.Null(await Relations().FindRelation(relation.ID));
        Assert.Empty(await Relations().GetRelationList(person.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyList(relation.ID));

        // The history has the link, for both of its elements, and the attribute.
        HistoryRelation old = Assert.Single(await Relations().GetRelationHistoryList(person.ID));
        Assert.Equal((relation.ID, Involved, person.ID, caseElement.ID, UserA, UserB, T0.AddDays(1)), (old.ID, old.RelationIdentifier, old.FromElementID, old.ToElementID, old.CreatedByUserID, old.EndedByUserID, old.TimeEnded));
        Assert.Equal([relation.ID], (await Relations().GetRelationHistoryList(caseElement.ID)).Select(x => x.ID));
        Assert.Equal(("role", UserB, T0.AddDays(1)),
            ((await _db.CreateRepository().GetPropertyHistoryList(relation.ID)).Single().PropertyIdentifier, (await _db.CreateRepository().GetPropertyHistoryList(relation.ID)).Single().EndedByUserID,
                (await _db.CreateRepository().GetPropertyHistoryList(relation.ID)).Single().TimeEnded));

        Assert.NotNull(await _db.CreateRepository().FindElement(person.ID));
        Assert.NotNull(await _db.CreateRepository().FindElement(caseElement.ID));
    }

    [Fact]
    public async Task EndingARelationThatIsNotThere_OrIsAlreadyEnded_ReturnsFalse()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = await Created(person.ID, caseElement.ID);

        Assert.False(await Relations().EndRelation(Guid.NewGuid(), UserA));
        Assert.True(await Relations().EndRelation(relation.ID, UserA));
        Assert.False(await Relations().EndRelation(relation.ID, UserA));
    }

    [Fact]
    public async Task EndingAnElement_EndsItsRelationsInBothDirections_WithTheirAttributes()
    {
        DataElement person = await Element();
        DataElement case1 = await Element("case");
        DataElement case2 = await Element("case");
        DataElement other = await Element();

        DataRelation outgoing = await Created(person.ID, case1.ID, Involved, T0, Role(Guid.Empty, "Witness"));
        DataRelation incoming = await Created(case2.ID, person.ID, "case-names-person", T0, Role(Guid.Empty, "Named"));
        DataRelation unrelated = await Created(other.ID, case1.ID, Involved, T0, Role(Guid.Empty, "Suspect"));
        ManualTimeProvider time = new(T0.AddDays(1));

        bool ended = await _db.CreateRepository(time).EndElement(person.ID, UserB);

        Assert.True(ended);

        // Both of its relations are gone from the current data, and in the history for both of their elements.
        Assert.Empty(await Relations().GetRelationList(person.ID));
        Assert.Equal([unrelated.ID], (await Relations().GetRelationList(case1.ID)).Select(x => x.ID));
        Assert.Empty(await Relations().GetRelationList(case2.ID));
        Assert.Equal([outgoing.ID, incoming.ID], (await Relations().GetRelationHistoryList(person.ID)).Select(x => x.ID).Order());
        Assert.All(await Relations().GetRelationHistoryList(person.ID), x => Assert.Equal((UserB, T0.AddDays(1)), (x.EndedByUserID, x.TimeEnded)));
        Assert.Single(await Relations().GetRelationHistoryList(case2.ID));

        // Their attributes went with them, and those of the relation that stayed did not.
        Assert.Empty(await _db.CreateRepository().GetPropertyList(outgoing.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyList(incoming.ID));
        Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(outgoing.ID));
        Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(incoming.ID));
        Assert.Single(await _db.CreateRepository().GetPropertyList(unrelated.ID));

        // The elements at the other ends are not ended.
        Assert.NotNull(await _db.CreateRepository().FindElement(case1.ID));
        Assert.NotNull(await _db.CreateRepository().FindElement(case2.ID));
    }

    [Fact]
    public async Task EndingAnElementThatIsRelatedToItself_EndsTheRelationOnce()
    {
        DataElement person = await Element();
        await Created(person.ID, person.ID, "person-knows-person");

        Assert.True(await _db.CreateRepository().EndElement(person.ID, UserA));

        Assert.Single(await Relations().GetRelationHistoryList(person.ID));
        Assert.Empty(await Relations().GetRelationList(person.ID));
    }

    [Fact]
    public async Task WhenARelationCannotBeMovedToTheHistory_TheElementDoesNotEndEither()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        DataRelation relation = await Created(person.ID, caseElement.ID);

        // The relation already has a row in the history with its ID, so moving it fails after the rest was moved.
        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(relation.ToHistory(UserA, T0));
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.CreateRepository().EndElement(person.ID, UserB));

        Assert.NotNull(await _db.CreateRepository().FindElement(person.ID));
        Assert.Null(await _db.CreateRepository().FindElementHistory(person.ID));
        Assert.Single(await Relations().GetRelationList(person.ID));
    }

    [Fact]
    public async Task ARelationOfAnElementThatEnded_CannotBeMadeAnyMore()
    {
        DataElement person = await Element();
        DataElement caseElement = await Element("case");
        await _db.CreateRepository().EndElement(person.ID, UserA);

        Assert.Equal(RelationCreateStatusEnum.FromElementNotFound, (await Relations().CreateRelation(Link(person.ID, caseElement.ID), [], null, null)).Status);
    }

    #endregion -- Ending --
}

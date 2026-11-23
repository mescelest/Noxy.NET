// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the repository calls are left on their default token.

#pragma warning disable xUnit1051

using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.Test.EntityManagement.Persistence;

public class DataRepositoryBatchTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private static DataElement NewElement() => new() { SchemaIdentifier = "person", CreatedByUserID = UserA, TimeCreated = T0 };

    private static DataPropertyString Name(Guid owner, string value, DateTime? time = null, Guid? user = null, string identifier = "name")
    {
        return new() { OwnerID = owner, PropertyIdentifier = identifier, Value = value, CreatedByUserID = user ?? UserA, TimeCreated = time ?? T0 };
    }

    private static DataPropertyInteger Age(Guid owner, long value, DateTime? time = null)
    {
        return new() { OwnerID = owner, PropertyIdentifier = "age", Value = value, CreatedByUserID = UserA, TimeCreated = time ?? T0 };
    }

    #region -- Creating an element with its values --

    [Fact]
    public async Task AnElement_IsCreatedWithItsValues_InOneStep()
    {
        DataElement element = NewElement();

        await _db.CreateRepository().CreateElement(element, [Name(element.ID, "Ann"), Age(element.ID, 41)]);

        Assert.NotNull(await _db.CreateRepository().FindElement(element.ID));
        List<DataProperty> list = await _db.CreateRepository().GetPropertyList(element.ID);
        Assert.Equal(["age", "name"], list.Select(x => x.PropertyIdentifier));
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(element.ID));
    }

    [Fact]
    public async Task AnElement_CanBeCreatedWithoutValues()
    {
        DataElement element = NewElement();

        await _db.CreateRepository().CreateElement(element, []);

        Assert.NotNull(await _db.CreateRepository().FindElement(element.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyList(element.ID));
    }

    [Fact]
    public async Task AValueOfAnotherElement_OrARepeatedProperty_IsRefused_AndNothingIsStored()
    {
        DataElement element = NewElement();

        await Assert.ThrowsAsync<ArgumentException>(() => _db.CreateRepository().CreateElement(element, [Name(Guid.NewGuid(), "Ann")]));
        await Assert.ThrowsAsync<ArgumentException>(() => _db.CreateRepository().CreateElement(element, [Name(element.ID, "Ann"), Name(element.ID, "Anna")]));

        Assert.Null(await _db.CreateRepository().FindElement(element.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyList(element.ID));
    }

    [Fact]
    public async Task WhenAValueCannotBeStored_TheElementIsNotCreatedEither()
    {
        DataElement element = NewElement();
        DataPropertyString name = Name(element.ID, "Ann");

        // A row with the ID of the value is already in the table, so storing it fails after the element was added.
        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(new DataPropertyString { ID = name.ID, OwnerID = Guid.NewGuid(), PropertyIdentifier = "other", Value = "x", CreatedByUserID = UserA });
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.CreateRepository().CreateElement(element, [name, Age(element.ID, 41)]));

        Assert.Null(await _db.CreateRepository().FindElement(element.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyList(element.ID));
    }

    #endregion -- Creating an element with its values --

    #region -- Reading the values of several elements --

    [Fact]
    public async Task TheValuesOfSeveralElements_AreReadInOneStep_ByOwnerThenProperty()
    {
        Guid ann = Guid.NewGuid();
        Guid bob = Guid.NewGuid();
        Guid cy = Guid.NewGuid();
        DataRepository repository = _db.CreateRepository();
        await repository.SetProperty(Name(ann, "Ann"));
        await repository.SetProperty(Age(ann, 41));
        await repository.SetProperty(Name(bob, "Bob"));
        await repository.SetProperty(Name(cy, "Cy"));

        List<DataProperty> list = await _db.CreateRepository().GetPropertyList([bob, ann]);

        Assert.Equal(3, list.Count);
        Assert.DoesNotContain(list, x => x.OwnerID == cy);
        Assert.Equal(list.OrderBy(x => x.OwnerID).ThenBy(x => x.PropertyIdentifier).Select(x => x.ID), list.Select(x => x.ID));
    }

    [Fact]
    public async Task NoElements_HaveNoValues()
    {
        Assert.Empty(await _db.CreateRepository().GetPropertyList(new List<Guid>()));
        Assert.Empty(await _db.CreateRepository().GetPropertyList([Guid.NewGuid()]));
    }

    #endregion -- Reading the values of several elements --

    #region -- Changing several values --

    private async Task<DataElement> ElementWith(params DataProperty[] properties)
    {
        DataElement element = NewElement();
        await _db.CreateRepository().CreateElement(element, [.. properties.Select(x => Rebased(x, element.ID))]);
        return element;
    }

    // The same value, as a value of the element.
    private static DataProperty Rebased(DataProperty property, Guid owner) => property switch
    {
        DataPropertyString x => new DataPropertyString { OwnerID = owner, PropertyIdentifier = x.PropertyIdentifier, Value = x.Value, CreatedByUserID = x.CreatedByUserID, TimeCreated = x.TimeCreated },
        DataPropertyInteger x => new DataPropertyInteger { OwnerID = owner, PropertyIdentifier = x.PropertyIdentifier, Value = x.Value, CreatedByUserID = x.CreatedByUserID, TimeCreated = x.TimeCreated },
        _ => throw new NotSupportedException()
    };

    [Fact]
    public async Task SeveralValues_AreSetAndCleared_InOneStep_EachOldOneMovingToTheHistory()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"), Age(Guid.Empty, 41), Name(Guid.Empty, "Smith", identifier: "surname"));
        ManualTimeProvider time = new(T0.AddHours(2));

        bool changed = await _db.CreateRepository(time).ChangeElementProperties(
            element.ID,
            [Name(element.ID, "Anna", T0.AddHours(1), UserB), Name(element.ID, "Jones", T0.AddHours(1), UserB, "nickname")],
            ["age"],
            UserB);

        Assert.True(changed);

        List<DataProperty> current = await _db.CreateRepository().GetPropertyList(element.ID);
        Assert.Equal([("name", "Anna"), ("nickname", "Jones"), ("surname", "Smith")], current.Cast<DataPropertyString>().Select(x => (x.PropertyIdentifier, x.Value)));

        List<HistoryProperty> history = await _db.CreateRepository().GetPropertyHistoryList(element.ID);
        Assert.Equal(["name", "age"], history.Select(x => x.PropertyIdentifier));

        // The replaced value ended when the new one began, and the cleared value when the call was made.
        Assert.Equal((T0.AddHours(1), UserB), (history[0].TimeEnded, history[0].EndedByUserID));
        Assert.Equal((T0.AddHours(2), UserB), (history[1].TimeEnded, history[1].EndedByUserID));
    }

    [Fact]
    public async Task ClearingAPropertyThatHasNoValue_IsIgnored()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"));

        bool changed = await _db.CreateRepository().ChangeElementProperties(element.ID, [], ["age", "nothing"], UserB);

        Assert.True(changed);
        Assert.Single(await _db.CreateRepository().GetPropertyList(element.ID));
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(element.ID));
    }

    [Fact]
    public async Task NothingToChange_ChangesNothing()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"));

        Assert.True(await _db.CreateRepository().ChangeElementProperties(element.ID, [], [], UserB));
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(element.ID));
    }

    [Fact]
    public async Task AnElementThatDoesNotExist_OrHasEnded_GetsNoValues()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"));
        await _db.CreateRepository().EndElement(element.ID, UserA);

        Assert.False(await _db.CreateRepository().ChangeElementProperties(element.ID, [Name(element.ID, "Anna")], [], UserB));
        Assert.False(await _db.CreateRepository().ChangeElementProperties(Guid.NewGuid(), [], ["name"], UserB));

        Assert.Empty(await _db.CreateRepository().GetPropertyList(element.ID));
    }

    [Fact]
    public async Task APropertyInBothLists_OrTwiceInOne_OrOfAnotherElement_IsRefused_AndNothingChanges()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"));

        await Assert.ThrowsAsync<ArgumentException>(() => _db.CreateRepository().ChangeElementProperties(element.ID, [Name(element.ID, "Anna")], ["name"], UserB));
        await Assert.ThrowsAsync<ArgumentException>(() => _db.CreateRepository().ChangeElementProperties(element.ID, [Name(element.ID, "Anna"), Name(element.ID, "Anne")], [], UserB));
        await Assert.ThrowsAsync<ArgumentException>(() => _db.CreateRepository().ChangeElementProperties(element.ID, [], ["name", "name"], UserB));
        await Assert.ThrowsAsync<ArgumentException>(() => _db.CreateRepository().ChangeElementProperties(element.ID, [Name(Guid.NewGuid(), "Anna")], [], UserB));

        Assert.Equal("Ann", Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(element.ID))).Value);
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(element.ID));
    }

    [Fact]
    public async Task ADifferentTypeOfAValue_IsRefused_AndNothingChanges()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"), Age(Guid.Empty, 41));
        DataPropertyInteger wrongType = new() { OwnerID = element.ID, PropertyIdentifier = "name", Value = 5, CreatedByUserID = UserB, TimeCreated = T0.AddHours(1) };

        // The first value is fine, the second one is of the wrong type: the first must not be kept.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _db.CreateRepository().ChangeElementProperties(element.ID, [Age(element.ID, 42, T0.AddHours(1)), wrongType], [], UserB));

        List<DataProperty> current = await _db.CreateRepository().GetPropertyList(element.ID);
        Assert.Equal(41, current.OfType<DataPropertyInteger>().Single().Value);
        Assert.Equal("Ann", current.OfType<DataPropertyString>().Single().Value);
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(element.ID));
    }

    [Fact]
    public async Task WhenTheHistoryCannotBeWritten_NoneOfTheChangesHappen()
    {
        DataElement element = await ElementWith(Name(Guid.Empty, "Ann"), Age(Guid.Empty, 41));
        List<DataProperty> current = await _db.CreateRepository().GetPropertyList(element.ID);

        // The age already has a row in the history with its ID, so moving it fails after the name has moved.
        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(current.OfType<DataPropertyInteger>().Single().ToHistory(UserA, T0));
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.CreateRepository().ChangeElementProperties(element.ID, [Name(element.ID, "Anna", T0.AddHours(1), UserB)], ["age"], UserB));

        List<DataProperty> after = await _db.CreateRepository().GetPropertyList(element.ID);
        Assert.Equal(current.Select(x => x.ID), after.Select(x => x.ID));
        Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(element.ID));
    }

    #endregion -- Changing several values --
}

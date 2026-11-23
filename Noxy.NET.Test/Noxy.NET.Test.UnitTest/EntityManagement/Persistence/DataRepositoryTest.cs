// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the repository calls are left on their default token.

#pragma warning disable xUnit1051

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.Test.EntityManagement.Persistence;

public class DataRepositoryTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private static DataElement NewElement(string schemaIdentifier = "person", DateTime? time = null)
    {
        return new() { SchemaIdentifier = schemaIdentifier, CreatedByUserID = UserA, TimeCreated = time ?? DateTime.UtcNow };
    }

    private static DataPropertyString Name(Guid ownerID, string value, Guid? user = null, DateTime? time = null, string identifier = "name")
    {
        return new() { OwnerID = ownerID, PropertyIdentifier = identifier, Value = value, CreatedByUserID = user ?? UserA, TimeCreated = time ?? DateTime.UtcNow };
    }

    #region -- Elements --

    [Fact]
    public async Task AnElement_CanBeCreated_AndFound()
    {
        DataElement created = await _db.CreateRepository().CreateElement(NewElement("person", T0));

        DataElement? found = await _db.CreateRepository().FindElement(created.ID);

        Assert.NotNull(found);
        Assert.Equal(("person", UserA, T0), (found.SchemaIdentifier, found.CreatedByUserID, found.TimeCreated));
        Assert.Equal(DateTimeKind.Utc, found.TimeCreated.Kind);
    }

    [Fact]
    public async Task AnElementThatIsNotThere_IsNotFound()
    {
        Assert.Null(await _db.CreateRepository().FindElement(Guid.NewGuid()));
        Assert.Null(await _db.CreateRepository().FindElementHistory(Guid.NewGuid()));
    }

    [Fact]
    public async Task TheElementsOfOneKind_AreListedInTheOrderTheyWereMade_APageAtATime()
    {
        DataRepository repository = _db.CreateRepository();
        List<DataElement> persons = [];
        for (int i = 0; i < 5; i++)
        {
            persons.Add(await repository.CreateElement(NewElement("person", T0.AddMinutes(i))));
        }

        await repository.CreateElement(NewElement("case", T0));

        Assert.Equal(5, await _db.CreateRepository().GetElementCount("person"));
        Assert.Equal(1, await _db.CreateRepository().GetElementCount("case"));
        Assert.Equal(0, await _db.CreateRepository().GetElementCount("vehicle"));

        Assert.Equal(persons.Select(x => x.ID), (await _db.CreateRepository().GetElementList("person", 0, 100)).Select(x => x.ID));
        Assert.Equal(persons.Skip(1).Take(2).Select(x => x.ID), (await _db.CreateRepository().GetElementList("person", 1, 2)).Select(x => x.ID));
        Assert.Empty(await _db.CreateRepository().GetElementList("person", 5, 10));
    }

    [Fact]
    public async Task AnElement_CannotBeListedWithANegativePage()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _db.CreateRepository().GetElementList("person", -1, 10));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _db.CreateRepository().GetElementList("person", 0, -1));
    }

    [Fact]
    public async Task EndingAnElement_MovesItAndAllOfItsPropertiesToTheHistory_AndNothingElse()
    {
        ManualTimeProvider time = new(T0.AddHours(1));
        DataElement person = await _db.CreateRepository(time).CreateElement(NewElement());
        DataElement other = await _db.CreateRepository(time).CreateElement(NewElement());

        await _db.CreateRepository(time).SetProperty(Name(person.ID, "Ann"));
        await _db.CreateRepository(time).SetProperty(new DataPropertyInteger { OwnerID = person.ID, PropertyIdentifier = "age", Value = 41, CreatedByUserID = UserA });
        await _db.CreateRepository(time).SetProperty(Name(other.ID, "Bob"));

        bool ended = await _db.CreateRepository(time).EndElement(person.ID, UserB);

        Assert.True(ended);
        DataRepository repository = _db.CreateRepository();
        Assert.Null(await repository.FindElement(person.ID));
        Assert.Empty(await repository.GetPropertyList(person.ID));

        HistoryElement? history = await repository.FindElementHistory(person.ID);
        Assert.NotNull(history);
        Assert.Equal((person.ID, "person", UserA, UserB, T0.AddHours(1)), (history.ID, history.SchemaIdentifier, history.CreatedByUserID, history.EndedByUserID, history.TimeEnded));

        List<HistoryProperty> propertyHistory = await repository.GetPropertyHistoryList(person.ID);
        Assert.Equal(["age", "name"], propertyHistory.Select(x => x.PropertyIdentifier).Order());
        Assert.All(propertyHistory, x => Assert.Equal((UserB, T0.AddHours(1)), (x.EndedByUserID, x.TimeEnded)));

        Assert.NotNull(await repository.FindElement(other.ID));
        Assert.Single(await repository.GetPropertyList(other.ID));
        Assert.Empty(await repository.GetPropertyHistoryList(other.ID));
    }

    [Fact]
    public async Task EndingAnElementThatIsNotThere_OrIsAlreadyEnded_ReturnsFalse()
    {
        DataElement person = await _db.CreateRepository().CreateElement(NewElement());

        Assert.False(await _db.CreateRepository().EndElement(Guid.NewGuid(), UserA));
        Assert.True(await _db.CreateRepository().EndElement(person.ID, UserA));
        Assert.False(await _db.CreateRepository().EndElement(person.ID, UserA));
    }

    [Fact]
    public async Task AnElementWithoutProperties_CanBeEnded()
    {
        DataElement person = await _db.CreateRepository().CreateElement(NewElement());

        Assert.True(await _db.CreateRepository().EndElement(person.ID, UserA));
        Assert.NotNull(await _db.CreateRepository().FindElementHistory(person.ID));
    }

    #endregion -- Elements --

    #region -- Setting a property --

    [Fact]
    public async Task TheFirstValueOfAProperty_IsStored_WithoutAnyHistory()
    {
        Guid owner = Guid.NewGuid();

        await _db.CreateRepository().SetProperty(Name(owner, "Ann"));

        List<DataProperty> list = await _db.CreateRepository().GetPropertyList(owner);
        DataPropertyString value = Assert.IsType<DataPropertyString>(Assert.Single(list));
        Assert.Equal(("name", "Ann", UserA), (value.PropertyIdentifier, value.Value, value.CreatedByUserID));
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(owner));
    }

    [Fact]
    public async Task ANewValue_BecomesTheCurrentOne_AndTheOldRowMovesToTheHistory()
    {
        Guid owner = Guid.NewGuid();
        DataPropertyString first = await _db.CreateRepository().SetProperty(Name(owner, "Ann", UserA, T0));
        DataPropertyString second = await _db.CreateRepository().SetProperty(Name(owner, "Anna", UserB, T0.AddMinutes(5)));

        DataPropertyString current = Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner)));
        Assert.Equal((second.ID, "Anna", UserB), (current.ID, current.Value, current.CreatedByUserID));
        Assert.NotEqual(first.ID, second.ID);

        HistoryPropertyString old = Assert.IsType<HistoryPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(owner)));
        Assert.Equal((first.ID, "Ann", UserA, T0), (old.ID, old.Value, old.CreatedByUserID, old.TimeCreated));

        // It ended when the new one began, and by the user who made the new one.
        Assert.Equal((T0.AddMinutes(5), UserB), (old.TimeEnded, old.EndedByUserID));
    }

    [Fact]
    public async Task EveryChange_AddsOneRowToTheHistory_InTheOrderTheyEnded()
    {
        Guid owner = Guid.NewGuid();
        foreach ((string value, int minute) in new[] { ("a", 0), ("b", 1), ("c", 2), ("d", 3) })
        {
            await _db.CreateRepository().SetProperty(Name(owner, value, time: T0.AddMinutes(minute)));
        }

        Assert.Equal("d", Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner))).Value);

        List<HistoryProperty> history = await _db.CreateRepository().GetPropertyHistoryList(owner);
        Assert.Equal(["a", "b", "c"], history.Cast<HistoryPropertyString>().Select(x => x.Value));
        Assert.Equal([T0.AddMinutes(1), T0.AddMinutes(2), T0.AddMinutes(3)], history.Select(x => x.TimeEnded));
    }

    [Fact]
    public async Task TheSamePropertyOfTwoOwners_AndTwoPropertiesOfOneOwner_AreSeparate()
    {
        Guid ann = Guid.NewGuid();
        Guid bob = Guid.NewGuid();

        await _db.CreateRepository().SetProperty(Name(ann, "Ann"));
        await _db.CreateRepository().SetProperty(Name(bob, "Bob"));
        await _db.CreateRepository().SetProperty(Name(ann, "Smith", identifier: "surname"));

        Assert.Equal(["Ann", "Smith"], (await _db.CreateRepository().GetPropertyList(ann)).Cast<DataPropertyString>().Select(x => x.Value));
        Assert.Equal(["Bob"], (await _db.CreateRepository().GetPropertyList(bob)).Cast<DataPropertyString>().Select(x => x.Value));
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(ann));
    }

    [Fact]
    public async Task ThePropertiesOfOneOwner_AreListedWhateverTheirType_ByIdentifier()
    {
        Guid owner = Guid.NewGuid();
        DataRepository repository = _db.CreateRepository();
        await repository.SetProperty(new DataPropertyInteger { OwnerID = owner, PropertyIdentifier = "c-age", Value = 41, CreatedByUserID = UserA });
        await repository.SetProperty(new DataPropertyBoolean { OwnerID = owner, PropertyIdentifier = "a-alive", Value = true, CreatedByUserID = UserA });
        await repository.SetProperty(Name(owner, "Ann", identifier: "b-name"));
        await repository.SetProperty(new DataPropertyBoolean { OwnerID = Guid.NewGuid(), PropertyIdentifier = "a-alive", Value = false, CreatedByUserID = UserA });

        List<DataProperty> list = await _db.CreateRepository().GetPropertyList(owner);

        Assert.Equal(["a-alive", "b-name", "c-age"], list.Select(x => x.PropertyIdentifier));
        Assert.Equal([typeof(DataPropertyBoolean), typeof(DataPropertyString), typeof(DataPropertyInteger)], list.Select(x => x.GetType()));
    }

    [Fact]
    public async Task TheHistoryOfOneOwner_IsListedWhateverTheTypeOfTheValues_InTheOrderTheyEnded()
    {
        Guid owner = Guid.NewGuid();
        DataRepository repository = _db.CreateRepository();
        await repository.SetProperty(new DataPropertyInteger { OwnerID = owner, PropertyIdentifier = "age", Value = 40, CreatedByUserID = UserA, TimeCreated = T0 });
        await repository.SetProperty(Name(owner, "Ann", time: T0));
        await repository.SetProperty(new DataPropertyInteger { OwnerID = owner, PropertyIdentifier = "age", Value = 41, CreatedByUserID = UserA, TimeCreated = T0.AddMinutes(2) });
        await repository.SetProperty(Name(owner, "Anna", time: T0.AddMinutes(1)));

        List<HistoryProperty> history = await _db.CreateRepository().GetPropertyHistoryList(owner);

        Assert.Equal(["name", "age"], history.Select(x => x.PropertyIdentifier));
        Assert.Equal([typeof(HistoryPropertyString), typeof(HistoryPropertyInteger)], history.Select(x => x.GetType()));
    }

    #endregion -- Setting a property --

    #region -- Clearing a property --

    [Fact]
    public async Task AClearedProperty_HasNoCurrentValue_AndItsLastValueIsInTheHistory()
    {
        Guid owner = Guid.NewGuid();
        ManualTimeProvider time = new(T0.AddHours(2));
        DataPropertyString set = await _db.CreateRepository().SetProperty(Name(owner, "Ann", time: T0));

        bool cleared = await _db.CreateRepository(time).ClearProperty<DataPropertyString>(owner, "name", UserB);

        Assert.True(cleared);
        Assert.Empty(await _db.CreateRepository().GetPropertyList(owner));
        HistoryPropertyString old = Assert.IsType<HistoryPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(owner)));
        Assert.Equal((set.ID, "Ann", UserB, T0.AddHours(2)), (old.ID, old.Value, old.EndedByUserID, old.TimeEnded));
    }

    [Fact]
    public async Task ClearingAPropertyWithoutAValue_OrOfTheWrongType_ReturnsFalseAndChangesNothing()
    {
        Guid owner = Guid.NewGuid();
        await _db.CreateRepository().SetProperty(Name(owner, "Ann"));

        Assert.False(await _db.CreateRepository().ClearProperty<DataPropertyString>(owner, "nothing", UserA));
        Assert.False(await _db.CreateRepository().ClearProperty<DataPropertyString>(Guid.NewGuid(), "name", UserA));
        Assert.False(await _db.CreateRepository().ClearProperty<DataPropertyInteger>(owner, "name", UserA));

        Assert.Single(await _db.CreateRepository().GetPropertyList(owner));
        Assert.Empty(await _db.CreateRepository().GetPropertyHistoryList(owner));
    }

    [Fact]
    public async Task AClearedProperty_CanBeSetAgain()
    {
        Guid owner = Guid.NewGuid();
        await _db.CreateRepository().SetProperty(Name(owner, "Ann"));
        await _db.CreateRepository().ClearProperty<DataPropertyString>(owner, "name", UserA);

        await _db.CreateRepository().SetProperty(Name(owner, "Anna"));

        Assert.Equal("Anna", Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner))).Value);
        Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(owner));
    }

    #endregion -- Clearing a property --

    #region -- Every type holds its value --

    [Fact]
    public async Task ABoolean_IsStored_AlsoWhenItIsFalse()
    {
        Guid owner = Guid.NewGuid();
        await _db.CreateRepository().SetProperty(new DataPropertyBoolean { OwnerID = owner, PropertyIdentifier = "alive", Value = true, CreatedByUserID = UserA });
        await _db.CreateRepository().SetProperty(new DataPropertyBoolean { OwnerID = owner, PropertyIdentifier = "alive", Value = false, CreatedByUserID = UserA });

        Assert.False(Assert.IsType<DataPropertyBoolean>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner))).Value);
        Assert.True(Assert.IsType<HistoryPropertyBoolean>(Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(owner))).Value);
    }

    [Fact]
    public async Task ADateTime_IsStoredInUtc()
    {
        Guid owner = Guid.NewGuid();
        DateTime born = new(1985, 3, 14, 8, 30, 15, DateTimeKind.Utc);
        await _db.CreateRepository().SetProperty(new DataPropertyDateTime { OwnerID = owner, PropertyIdentifier = "born", Value = born, CreatedByUserID = UserA });

        DataPropertyDateTime value = Assert.IsType<DataPropertyDateTime>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner)));

        Assert.Equal(born, value.Value);
        Assert.Equal(DateTimeKind.Utc, value.Value.Kind);
    }

    [Fact]
    public async Task ALocalDateTime_IsStoredAsTheSameMomentInUtc()
    {
        Guid owner = Guid.NewGuid();
        DateTime local = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Local);
        await _db.CreateRepository().SetProperty(new DataPropertyDateTime { OwnerID = owner, PropertyIdentifier = "seen", Value = local, CreatedByUserID = UserA });

        DataPropertyDateTime value = Assert.IsType<DataPropertyDateTime>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner)));

        Assert.Equal(local.ToUniversalTime(), value.Value);
    }

    [Fact]
    public async Task ADecimal_KeepsAllOfItsDigits()
    {
        Guid owner = Guid.NewGuid();
        decimal value = 123456789.123456789012345678m;
        await _db.CreateRepository().SetProperty(new DataPropertyDecimal { OwnerID = owner, PropertyIdentifier = "weight", Value = value, CreatedByUserID = UserA });

        Assert.Equal(value, Assert.IsType<DataPropertyDecimal>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner))).Value);
    }

    [Fact]
    public async Task AnInteger_CanBeAsLargeAsA64BitNumber()
    {
        Guid owner = Guid.NewGuid();
        await _db.CreateRepository().SetProperty(new DataPropertyInteger { OwnerID = owner, PropertyIdentifier = "big", Value = long.MaxValue, CreatedByUserID = UserA });
        await _db.CreateRepository().SetProperty(new DataPropertyInteger { OwnerID = owner, PropertyIdentifier = "small", Value = long.MinValue, CreatedByUserID = UserA });

        List<DataProperty> list = await _db.CreateRepository().GetPropertyList(owner);

        Assert.Equal([long.MaxValue, long.MinValue], list.Cast<DataPropertyInteger>().Select(x => x.Value));
    }

    [Fact]
    public async Task AnImage_IsStoredAsItsReference()
    {
        Guid owner = Guid.NewGuid();
        await _db.CreateRepository().SetProperty(new DataPropertyImage { OwnerID = owner, PropertyIdentifier = "picture", Value = "images/2026/ann.png", CreatedByUserID = UserA });

        Assert.Equal("images/2026/ann.png", Assert.IsType<DataPropertyImage>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner))).Value);
    }

    [Fact]
    public async Task AString_CanBeVeryLong_AndKeepsItsLineBreaksAndSpecialCharacters()
    {
        Guid owner = Guid.NewGuid();
        string text = string.Concat(Enumerable.Repeat("Æble\r\nøl, 'quoted' \"double\" 🙂 ", 5000));
        await _db.CreateRepository().SetProperty(Name(owner, text));

        Assert.Equal(text, Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner))).Value);
    }

    #endregion -- Every type holds its value --

    #region -- All or nothing --

    [Fact]
    public async Task WhenTheHistoryCannotBeWritten_TheChangeDoesNotHappenAtAll()
    {
        Guid owner = Guid.NewGuid();
        DataPropertyString first = await _db.CreateRepository().SetProperty(Name(owner, "Ann"));

        // A row with the ID of the one that would move is already in the history, so the move fails halfway through.
        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(first.ToHistory(UserA, T0));
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.CreateRepository().SetProperty(Name(owner, "Anna")));

        DataPropertyString current = Assert.IsType<DataPropertyString>(Assert.Single(await _db.CreateRepository().GetPropertyList(owner)));
        Assert.Equal((first.ID, "Ann"), (current.ID, current.Value));
        Assert.Single(await _db.CreateRepository().GetPropertyHistoryList(owner));
    }

    [Fact]
    public async Task WhenAnElementCannotBeEnded_NothingOfItIsMoved()
    {
        DataElement person = await _db.CreateRepository().CreateElement(NewElement());
        DataPropertyString name = await _db.CreateRepository().SetProperty(Name(person.ID, "Ann"));

        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(name.ToHistory(UserA, T0));
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.CreateRepository().EndElement(person.ID, UserB));

        Assert.NotNull(await _db.CreateRepository().FindElement(person.ID));
        Assert.Null(await _db.CreateRepository().FindElementHistory(person.ID));
        Assert.Single(await _db.CreateRepository().GetPropertyList(person.ID));
    }

    [Fact]
    public async Task TheDatabase_RefusesTwoCurrentValuesOfOneProperty()
    {
        Guid owner = Guid.NewGuid();
        await using EntityManagementContext context = _db.CreateContext();
        context.Add(Name(owner, "Ann"));
        context.Add(Name(owner, "Anna"));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    #endregion -- All or nothing --

    #region -- The tables --

    private List<string> Query(string sql)
    {
        using SqliteCommand command = _db.Connection.CreateCommand();
        command.CommandText = sql;

        using SqliteDataReader reader = command.ExecuteReader();
        List<string> result = [];
        while (reader.Read())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    [Fact]
    public void TheMigration_CreatesTheCurrentTablesAndTheirHistoryTwins()
    {
        List<string> tables = Query("SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '\\_\\_%' ESCAPE '\\' ORDER BY name");

        string[] kinds = ["Boolean", "DateTime", "Decimal", "Image", "Integer", "String"];
        List<string> expected = ["DataElement", "HistoryElement", .. kinds.Select(x => $"DataProperty{x}"), .. kinds.Select(x => $"HistoryProperty{x}"), "DataRelation", "HistoryRelation", "DataTextParameter", "HistoryTextParameter"];

        Assert.Equal(expected.Order(), tables);
    }

    [Fact]
    public void NoTableOfTheData_HasAForeignKey()
    {
        List<string> tables = Query("SELECT name FROM sqlite_master WHERE type = 'table' AND (name LIKE 'Data%' OR name LIKE 'History%')");

        Assert.NotEmpty(tables);
        Assert.All(tables, table => Assert.Empty(Query($"SELECT \"table\" FROM pragma_foreign_key_list('{table}')")));
    }

    [Fact]
    public void EveryHistoryTable_HasTheColumnsOfItsCurrentTable_AndWhenAndWhoEnded()
    {
        string[] names = ["DataElement", "DataRelation", "DataTextParameter", "DataPropertyBoolean", "DataPropertyDateTime", "DataPropertyDecimal", "DataPropertyImage", "DataPropertyInteger", "DataPropertyString"];

        foreach (string name in names)
        {
            List<string> current = Query($"SELECT name FROM pragma_table_info('{name}')");
            List<string> history = Query($"SELECT name FROM pragma_table_info('{name.Replace("Data", "History", StringComparison.Ordinal)}')");

            Assert.Equal([.. current, "TimeEnded", "EndedByUserID"], history);
        }
    }

    [Fact]
    public void TheMigrations_MatchTheModel()
    {
        using EntityManagementContext context = _db.CreateContext();

        Assert.False(context.Database.HasPendingModelChanges(), "The model has changed since the last migration: add a migration (dotnet ef migrations add <Name>).");
    }

    [Fact]
    public void TheLongestIdentifierOfTheData_IsTheLongestOfTheSchema()
    {
        Assert.Equal(DefinitionIdentifier.MaxLength, BaseData.IdentifierMaxLength);
    }

    #endregion -- The tables --
}

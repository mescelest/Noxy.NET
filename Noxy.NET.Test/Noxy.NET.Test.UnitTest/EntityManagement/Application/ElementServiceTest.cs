// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the service calls are left on their default token.

#pragma warning disable xUnit1051

using Noxy.NET.EntityManagement.Application;
using Noxy.NET.EntityManagement.Application.Enums;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Repositories;
using Noxy.NET.Test.EntityManagement.Definitions;
using Noxy.NET.Test.EntityManagement.Persistence;

namespace Noxy.NET.Test.EntityManagement.Application;

public class ElementServiceTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = new();
    private readonly ManualTimeProvider _time = new(T0);
    private readonly DefinitionRegistry _definition;

    public ElementServiceTest()
    {
        // A person with a value of every type, and a case that has only two of the properties.
        _definition = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddTextParameter("title", TextKindEnum.Line);

            builder.AddProperty("name", PropertyKindEnum.String, "title", isRequired: true, maxLength: 20);
            builder.AddProperty("age", PropertyKindEnum.Integer, "title");
            builder.AddProperty("born", PropertyKindEnum.DateTime, "title");
            builder.AddProperty("weight", PropertyKindEnum.Decimal, "title");
            builder.AddProperty("alive", PropertyKindEnum.Boolean, "title");
            builder.AddProperty("picture", PropertyKindEnum.Image, "title");
            builder.AddProperty("note", PropertyKindEnum.String, "title");

            builder.AddElement("person", "title", ["name", "age", "born", "weight", "alive", "picture", "note"]);
            builder.AddElement("case", "title", ["name", "note"]);
        }));
    }

    public void Dispose() => _db.Dispose();

    // A service on a new context each time, so what it reads is what is in the database.
    private ElementService Service() => new(_definition, new DataRepository(_db.CreateContext(), _time), _time);

    private static Dictionary<string, object?> Values(params (string Identifier, object? Value)[] list) => list.ToDictionary(x => x.Identifier, x => x.Value);

    private static DataValidationException Errors(Func<Task> act)
    {
        return Assert.ThrowsAsync<DataValidationException>(act).GetAwaiter().GetResult();
    }

    #region -- Creating an element --

    [Fact]
    public async Task AnElement_IsCreatedWithTheValuesOfEveryType_AndTheyComeBackInTheirType()
    {
        DateTime born = new(1985, 3, 14, 8, 30, 0, DateTimeKind.Utc);

        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41), ("born", born), ("weight", 61.5m), ("alive", true), ("picture", "images/ann.png"), ("note", "Likes tea")), UserA);

        ElementModel? found = await Service().FindElement(created.ID);
        Assert.NotNull(found);
        Assert.Equal(("person", UserA, T0), (found.ElementIdentifier, found.CreatedByUserID, found.TimeCreated));
        Assert.Equal(7, found.PropertyMap.Count);

        Assert.Equal("Ann", found.PropertyMap["name"].Value);
        Assert.Equal(41L, found.PropertyMap["age"].Value);
        Assert.Equal(born, found.PropertyMap["born"].Value);
        Assert.Equal(61.5m, found.PropertyMap["weight"].Value);
        Assert.Equal(true, found.PropertyMap["alive"].Value);
        Assert.Equal("images/ann.png", found.PropertyMap["picture"].Value);

        Assert.Equal(PropertyKindEnum.Integer, found.PropertyMap["age"].Kind);
        Assert.Equal((UserA, T0), (found.PropertyMap["name"].CreatedByUserID, found.PropertyMap["name"].TimeCreated));
    }

    [Fact]
    public async Task TheReturnedModel_IsTheSameAsWhatIsStored()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41)), UserA);

        ElementModel stored = (await Service().FindElement(created.ID))!;

        Assert.Equal(created.PropertyMap.Keys.Order(), stored.PropertyMap.Keys.Order());
        Assert.All(created.PropertyMap, x => Assert.Equal(x.Value, stored.PropertyMap[x.Key]));
    }

    [Fact]
    public async Task TheValuesThatAreLeftOut_OrEmpty_AreNotStored()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", null), ("note", ""), ("picture", "   ")), UserA);

        Assert.Equal(["name"], created.PropertyMap.Keys);
        Assert.Single((await Service().FindElement(created.ID))!.PropertyMap);
    }

    [Fact]
    public async Task ARequiredProperty_MustHaveAValue()
    {
        DataValidationException none = Errors(() => Service().CreateElement("person", Values(("age", 41)), UserA));
        DataValidationException empty = Errors(() => Service().CreateElement("person", Values(("name", "  ")), UserA));
        DataValidationException nullValue = Errors(() => Service().CreateElement("person", Values(("name", null)), UserA));

        Assert.All(new[] { none, empty, nullValue }, x => Assert.Equal([new DataError("name", DataErrorEnum.Required, "The property 'name' is required.")], x.ErrorList));
        Assert.Equal(0, (await Service().GetElementPage("person", 0, 10)).TotalCount);
    }

    [Fact]
    public async Task APropertyThatTheElementDoesNotHave_IsReported_AlsoOneThatIsNotDefined()
    {
        DataValidationException exception = Errors(() => Service().CreateElement("case", Values(("name", "Case 1"), ("age", 41), ("nothing", "x")), UserA));

        Assert.Equal(["age", "nothing"], exception.ErrorList.Select(x => x.PropertyIdentifier).Order());
        Assert.All(exception.ErrorList, x => Assert.Equal(DataErrorEnum.UnknownProperty, x.Code));
    }

    [Theory]
    [InlineData("name", 5)]
    [InlineData("age", "forty")]
    [InlineData("age", 41.5)]
    [InlineData("age", true)]
    [InlineData("born", "1985-03-14")]
    [InlineData("born", 5)]
    [InlineData("weight", 61.5)]
    [InlineData("weight", 61.5f)]
    [InlineData("weight", "heavy")]
    [InlineData("alive", "yes")]
    [InlineData("alive", 1)]
    [InlineData("picture", 5)]
    public async Task AValueOfTheWrongType_IsReported(string property, object value)
    {
        // The name is required, so the other cases are given a good one beside the bad value.
        Dictionary<string, object?> values = property == "name" ? Values((property, value)) : Values(("name", "Ann"), (property, value));

        DataValidationException exception = Errors(() => Service().CreateElement("person", values, UserA));

        DataError error = Assert.Single(exception.ErrorList);
        Assert.Equal((property, DataErrorEnum.WrongType), (error.PropertyIdentifier, error.Code));
    }

    [Fact]
    public async Task EveryProblemIsReportedTogether_AndNothingIsStored()
    {
        DataValidationException exception = Errors(() => Service().CreateElement("person", Values(("age", "forty"), ("nothing", 1), ("weight", 61.5), ("note", 5)), UserA));

        // The name is missing, the age and the weight and the note are of the wrong type, and one property does not exist.
        Assert.Equal(5, exception.ErrorList.Count);
        Assert.Contains("5 problems", exception.Message);
        Assert.Equal(0, (await Service().GetElementPage("person", 0, 10)).TotalCount);
    }

    [Theory]
    [InlineData(typeof(byte), 41L)]
    [InlineData(typeof(sbyte), 41L)]
    [InlineData(typeof(short), 41L)]
    [InlineData(typeof(ushort), 41L)]
    [InlineData(typeof(int), 41L)]
    [InlineData(typeof(uint), 41L)]
    [InlineData(typeof(long), 41L)]
    [InlineData(typeof(ulong), 41L)]
    public async Task AWholeNumber_OfAnyType_IsAcceptedForAnInteger(Type type, long expected)
    {
        object value = Convert.ChangeType(41, type);

        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", value)), UserA);

        Assert.Equal(expected, created.PropertyMap["age"].Value);
    }

    [Fact]
    public async Task ATooLargeNumber_IsReported_ForAnInteger()
    {
        DataValidationException exception = Errors(() => Service().CreateElement("person", Values(("name", "Ann"), ("age", ulong.MaxValue)), UserA));

        Assert.Equal(DataErrorEnum.WrongType, Assert.Single(exception.ErrorList).Code);

        ElementModel big = await Service().CreateElement("person", Values(("name", "Ann"), ("age", long.MaxValue)), UserA);
        Assert.Equal(long.MaxValue, big.PropertyMap["age"].Value);
    }

    [Fact]
    public async Task AWholeNumber_IsAcceptedForADecimal_AndKeepsItsDigits()
    {
        ElementModel whole = await Service().CreateElement("person", Values(("name", "Ann"), ("weight", 61)), UserA);
        ElementModel exact = await Service().CreateElement("person", Values(("name", "Bob"), ("weight", 123456789.123456789012345678m)), UserA);

        Assert.Equal(61m, whole.PropertyMap["weight"].Value);
        Assert.Equal(123456789.123456789012345678m, (await Service().FindElement(exact.ID))!.PropertyMap["weight"].Value);
    }

    [Fact]
    public async Task ATime_IsKeptInUtc_WhateverItWasGivenAs()
    {
        DateTime utc = new(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime local = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Local);
        DateTime unspecified = new(2026, 6, 1, 10, 0, 0, DateTimeKind.Unspecified);
        DateTimeOffset offset = new(2026, 6, 1, 12, 0, 0, TimeSpan.FromHours(2));

        ElementModel a = await Service().CreateElement("person", Values(("name", "A"), ("born", utc)), UserA);
        ElementModel b = await Service().CreateElement("person", Values(("name", "B"), ("born", local)), UserA);
        ElementModel c = await Service().CreateElement("person", Values(("name", "C"), ("born", unspecified)), UserA);
        ElementModel d = await Service().CreateElement("person", Values(("name", "D"), ("born", offset)), UserA);

        Assert.Equal(utc, a.PropertyMap["born"].Value);
        Assert.Equal(local.ToUniversalTime(), b.PropertyMap["born"].Value);
        Assert.Equal(utc, c.PropertyMap["born"].Value);
        Assert.Equal(utc, d.PropertyMap["born"].Value);
        Assert.All(new[] { a, b, c, d }, x => Assert.Equal(DateTimeKind.Utc, ((DateTime)x.PropertyMap["born"].Value).Kind));
    }

    [Fact]
    public async Task AString_CanBeAsLongAsTheMaximum_ButNotLonger()
    {
        await Service().CreateElement("person", Values(("name", new string('a', 20))), UserA);

        DataValidationException exception = Errors(() => Service().CreateElement("person", Values(("name", new string('a', 21))), UserA));
        Assert.Equal(DataErrorEnum.TooLong, Assert.Single(exception.ErrorList).Code);

        // A property with no maximum still has the bound of the column.
        await Service().CreateElement("person", Values(("name", "Ann"), ("note", new string('a', BaseData.TextMaxLength))), UserA);
        Assert.Equal(DataErrorEnum.TooLong, Errors(() => Service().CreateElement("person", Values(("name", "Ann"), ("note", new string('a', BaseData.TextMaxLength + 1))), UserA)).ErrorList.Single().Code);
        Assert.Equal(DataErrorEnum.TooLong, Errors(() => Service().CreateElement("person", Values(("name", "Ann"), ("picture", new string('a', BaseData.TextMaxLength + 1))), UserA)).ErrorList.Single().Code);
    }

    [Fact]
    public async Task AnElementThatIsNotDefined_IsAMistakeOfTheCode()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().CreateElement("vehicle", Values(), UserA));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().GetElementPage("vehicle", 0, 10));
    }

    [Fact]
    public async Task ACase_NeedsOnlyItsOwnProperties()
    {
        ElementModel created = await Service().CreateElement("case", Values(("name", "Case 1"), ("note", "Open")), UserA);

        Assert.Equal(["name", "note"], created.PropertyMap.Keys.Order());
    }

    #endregion -- Creating an element --

    #region -- Finding and listing --

    [Fact]
    public async Task AnElementThatIsNotThere_IsNotFound()
    {
        Assert.Null(await Service().FindElement(Guid.NewGuid()));
    }

    [Fact]
    public async Task TheElementsOfOneKind_AreListedWithTheirValues_APageAtATime()
    {
        for (int i = 0; i < 5; i++)
        {
            _time.Now = T0.AddMinutes(i);
            await Service().CreateElement("person", Values(("name", $"Person {i}"), ("age", 20 + i)), UserA);
        }

        await Service().CreateElement("case", Values(("name", "Case 1")), UserA);

        ElementPageModel page = await Service().GetElementPage("person", 1, 2);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(["Person 1", "Person 2"], page.ElementList.Select(x => x.PropertyMap["name"].Value));
        Assert.Equal([21L, 22L], page.ElementList.Select(x => x.PropertyMap["age"].Value));
        Assert.Equal(1, (await Service().GetElementPage("case", 0, 10)).TotalCount);
        Assert.Empty((await Service().GetElementPage("person", 5, 10)).ElementList);
    }

    [Fact]
    public async Task AnElementWithoutValues_IsListedWithoutValues()
    {
        _definition.GetElement("person");
        await Service().CreateElement("case", Values(("name", "Case 1")), UserA);

        ElementPageModel page = await Service().GetElementPage("case", 0, 10);

        Assert.Single(page.ElementList);
        Assert.Equal(["name"], page.ElementList[0].PropertyMap.Keys);
    }

    #endregion -- Finding and listing --

    #region -- Changing an element --

    [Fact]
    public async Task ChangingAnElement_ChangesTheValuesThatAreGiven_AndLeavesTheOthers()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41), ("note", "Likes tea")), UserA);
        _time.Now = T0.AddHours(1);

        ElementModel? changed = await Service().ChangeElement(created.ID, Values(("name", "Anna"), ("age", 42)), UserB);

        Assert.NotNull(changed);
        Assert.Equal(("Anna", 42L, "Likes tea"), (changed.PropertyMap["name"].Value, changed.PropertyMap["age"].Value, changed.PropertyMap["note"].Value));
        Assert.Equal((UserB, T0.AddHours(1)), (changed.PropertyMap["name"].CreatedByUserID, changed.PropertyMap["name"].TimeCreated));
        Assert.Equal((UserA, T0), (changed.PropertyMap["note"].CreatedByUserID, changed.PropertyMap["note"].TimeCreated));

        // The change is in the database, and what was replaced is in the history.
        Assert.Equal("Anna", (await Service().FindElement(created.ID))!.PropertyMap["name"].Value);
        IReadOnlyList<PropertyHistoryModel> history = await Service().GetPropertyHistory(created.ID);
        Assert.Equal(["age", "name"], history.Select(x => x.PropertyIdentifier).Order());
        Assert.Equal(("Ann", UserA, UserB, T0.AddHours(1)),
            (history.Single(x => x.PropertyIdentifier == "name").Value, history.Single(x => x.PropertyIdentifier == "name").CreatedByUserID, history.Single(x => x.PropertyIdentifier == "name").EndedByUserID,
                history.Single(x => x.PropertyIdentifier == "name").TimeEnded));
    }

    [Fact]
    public async Task ChangingAValueToTheSameValue_IsNotAChange_AndLeavesNothingInTheHistory()
    {
        DateTime born = new(1985, 3, 14, 8, 30, 0, DateTimeKind.Utc);
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41), ("born", born), ("weight", 61.5m), ("alive", true)), UserA);
        _time.Now = T0.AddHours(1);

        // The same values, written another way: an int for a long, a decimal with another scale, a local time of the same moment.
        ElementModel? same = await Service().ChangeElement(created.ID, Values(("name", "Ann"), ("age", 41), ("born", born.ToLocalTime()), ("weight", 61.50m), ("alive", true)), UserB);

        Assert.NotNull(same);
        Assert.Empty(await Service().GetPropertyHistory(created.ID));
        Assert.Equal(T0, same.PropertyMap["name"].TimeCreated);
    }

    [Fact]
    public async Task OnlyTheValuesThatChanged_GoToTheHistory()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41)), UserA);

        await Service().ChangeElement(created.ID, Values(("name", "Ann"), ("age", 42)), UserB);

        Assert.Equal(["age"], (await Service().GetPropertyHistory(created.ID)).Select(x => x.PropertyIdentifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnEmptyValue_ClearsAnOptionalProperty_AndTheOldValueGoesToTheHistory(string? empty)
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("note", "Likes tea")), UserA);
        _time.Now = T0.AddHours(1);

        ElementModel? changed = await Service().ChangeElement(created.ID, Values(("note", empty)), UserB);

        Assert.NotNull(changed);
        Assert.DoesNotContain("note", changed.PropertyMap.Keys);
        PropertyHistoryModel old = Assert.Single(await Service().GetPropertyHistory(created.ID));
        Assert.Equal(("note", "Likes tea", UserB, T0.AddHours(1)), (old.PropertyIdentifier, old.Value, old.EndedByUserID, old.TimeEnded));
    }

    [Fact]
    public async Task ClearingAPropertyThatHasNoValue_ChangesNothing()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann")), UserA);

        await Service().ChangeElement(created.ID, Values(("note", null)), UserB);

        Assert.Empty(await Service().GetPropertyHistory(created.ID));
    }

    [Fact]
    public async Task ARequiredProperty_CannotBeCleared()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann")), UserA);

        DataValidationException exception = Errors(() => Service().ChangeElement(created.ID, Values(("name", null)), UserB));

        Assert.Equal(DataErrorEnum.Required, Assert.Single(exception.ErrorList).Code);
        Assert.Equal("Ann", (await Service().FindElement(created.ID))!.PropertyMap["name"].Value);
    }

    [Fact]
    public async Task ABadValue_StopsTheWholeChange_AndNothingIsChanged()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41)), UserA);

        DataValidationException exception = Errors(() => Service().ChangeElement(created.ID, Values(("name", "Anna"), ("age", "forty"), ("nothing", 1), ("note", 5)), UserB));

        Assert.Equal(3, exception.ErrorList.Count);
        ElementModel stored = (await Service().FindElement(created.ID))!;
        Assert.Equal(("Ann", 41L), (stored.PropertyMap["name"].Value, stored.PropertyMap["age"].Value));
        Assert.Empty(await Service().GetPropertyHistory(created.ID));
    }

    [Fact]
    public async Task ChangingAnElementThatIsNotThere_OrHasEnded_GivesNull()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann")), UserA);
        await Service().EndElement(created.ID, UserA);

        Assert.Null(await Service().ChangeElement(Guid.NewGuid(), Values(("name", "Anna")), UserB));
        Assert.Null(await Service().ChangeElement(created.ID, Values(("name", "Anna")), UserB));
        Assert.Null(await Service().FindElement(created.ID));
    }

    [Fact]
    public async Task ASetOfValuesWithNothingInIt_ChangesNothing()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann")), UserA);

        ElementModel? same = await Service().ChangeElement(created.ID, Values(), UserB);

        Assert.NotNull(same);
        Assert.Empty(await Service().GetPropertyHistory(created.ID));
    }

    [Fact]
    public async Task APropertyAddedLater_AppearsWithoutTouchingTheRest()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann")), UserA);

        ElementModel? changed = await Service().ChangeElement(created.ID, Values(("age", 41)), UserB);

        Assert.Equal(["age", "name"], changed!.PropertyMap.Keys.Order());
        Assert.Empty(await Service().GetPropertyHistory(created.ID));
    }

    #endregion -- Changing an element --

    #region -- Ending an element and its history --

    [Fact]
    public async Task AnElementThatEnds_IsGone_ButItsValuesAreStillInTheHistory()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41)), UserA);
        _time.Now = T0.AddDays(1);

        bool ended = await Service().EndElement(created.ID, UserB);

        Assert.True(ended);
        Assert.Null(await Service().FindElement(created.ID));
        Assert.Equal(0, (await Service().GetElementPage("person", 0, 10)).TotalCount);

        IReadOnlyList<PropertyHistoryModel> history = await Service().GetPropertyHistory(created.ID);
        Assert.Equal(["age", "name"], history.Select(x => x.PropertyIdentifier).Order());
        Assert.All(history, x => Assert.Equal((UserB, T0.AddDays(1)), (x.EndedByUserID, x.TimeEnded)));
    }

    [Fact]
    public async Task EndingAnElementThatIsNotThere_OrIsAlreadyEnded_GivesFalse()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann")), UserA);

        Assert.False(await Service().EndElement(Guid.NewGuid(), UserA));
        Assert.True(await Service().EndElement(created.ID, UserA));
        Assert.False(await Service().EndElement(created.ID, UserA));
    }

    [Fact]
    public async Task TheHistory_HasEveryValueThatEnded_WithItsTypeAndWhenAndWho()
    {
        ElementModel created = await Service().CreateElement("person", Values(("name", "Ann"), ("age", 41), ("weight", 61.5m)), UserA);
        _time.Now = T0.AddHours(1);
        await Service().ChangeElement(created.ID, Values(("age", 42)), UserB);
        _time.Now = T0.AddHours(2);
        await Service().ChangeElement(created.ID, Values(("name", "Anna"), ("age", 43)), UserA);
        _time.Now = T0.AddHours(3);
        await Service().EndElement(created.ID, UserB);

        IReadOnlyList<PropertyHistoryModel> history = await Service().GetPropertyHistory(created.ID);

        // In the order they ended: the first age, then the name and the second age, then the three that were left when the element ended.
        Assert.Equal(6, history.Count);
        Assert.Equal(history.OrderBy(x => x.TimeEnded).Select(x => x.TimeEnded), history.Select(x => x.TimeEnded));

        PropertyHistoryModel firstAge = history[0];
        Assert.Equal(("age", PropertyKindEnum.Integer, 41L, T0.AddHours(1), UserB), (firstAge.PropertyIdentifier, firstAge.Kind, firstAge.Value, firstAge.TimeEnded, firstAge.EndedByUserID));
        Assert.Contains(history, x => x is { PropertyIdentifier: "weight", Value: 61.5m, Kind: PropertyKindEnum.Decimal });
    }

    [Fact]
    public async Task AnElementThatNeverHadValues_HasNoHistory()
    {
        Assert.Empty(await Service().GetPropertyHistory(Guid.NewGuid()));
    }

    #endregion -- Ending an element and its history --
}

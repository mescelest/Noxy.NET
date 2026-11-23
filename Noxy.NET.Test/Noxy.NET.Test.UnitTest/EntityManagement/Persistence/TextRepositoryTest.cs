// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the repository calls are left on their default token.

#pragma warning disable xUnit1051

using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.Test.EntityManagement.Persistence;

public class TextRepositoryTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private static DataTextParameter Text(string textIdentifier, string culture, string value, Guid? user = null, DateTime? time = null)
    {
        return new() { TextIdentifier = textIdentifier, Culture = culture, Value = value, CreatedByUserID = user ?? UserA, TimeCreated = time ?? DateTime.UtcNow };
    }

    private TextRepository Repository(TimeProvider? timeProvider = null) => new(_db.CreateContext(), timeProvider ?? TimeProvider.System);

    #region -- Setting and finding --

    [Fact]
    public async Task AText_CanBeSet_AndFound()
    {
        DataTextParameter set = await Repository().SetText(Text("person-title", "en", "Person", time: T0));

        DataTextParameter? found = await Repository().FindText("person-title", "en");

        Assert.NotNull(found);
        Assert.Equal((set.ID, "person-title", "en", "Person", UserA, T0), (found.ID, found.TextIdentifier, found.Culture, found.Value, found.CreatedByUserID, found.TimeCreated));
    }

    [Fact]
    public async Task AText_ThatHasNoValue_OrNoValueInThatCulture_IsNotFound()
    {
        await Repository().SetText(Text("person-title", "en", "Person"));

        Assert.Null(await Repository().FindText("nothing", "en"));
        Assert.Null(await Repository().FindText("person-title", "da"));
    }

    [Theory]
    [InlineData("da-DK", "da-DK")]
    [InlineData("da-DK", "da-dk")]
    [InlineData("da-DK", "DA-DK")]
    [InlineData("en", "EN")]
    public async Task TheCulture_IsComparedWithoutRegardToCase(string stored, string asked)
    {
        await Repository().SetText(Text("person-title", stored, "Person"));

        DataTextParameter? found = await Repository().FindText("person-title", asked);

        Assert.NotNull(found);
        Assert.Equal(stored, found.Culture);
    }

    [Fact]
    public async Task TheTextIdentifier_IsComparedExactly()
    {
        await Repository().SetText(Text("person-title", "en", "Person"));

        Assert.Null(await Repository().FindText("PERSON-TITLE", "en"));
    }

    [Fact]
    public async Task ATextInOneCulture_DoesNotTouchTheSameTextInAnother()
    {
        await Repository().SetText(Text("person-title", "en", "Person"));
        await Repository().SetText(Text("person-title", "da", "Person (dansk)"));
        await Repository().SetText(Text("person-title", "en", "Human"));

        Assert.Equal("Human", (await Repository().FindText("person-title", "en"))!.Value);
        Assert.Equal("Person (dansk)", (await Repository().FindText("person-title", "da"))!.Value);
        Assert.Empty(await Repository().GetTextHistoryList("person-title", "da"));
        Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
    }

    [Fact]
    public async Task AValueWithRichTextAndSpecialCharacters_IsKeptAsItIs()
    {
        string value = string.Concat(Enumerable.Repeat("<p>Æble <b>&amp;</b>\r\nøl 'quoted' \"double\" 🙂</p>", 2000));
        await Repository().SetText(Text("intro", "da", value));

        Assert.Equal(value, (await Repository().FindText("intro", "da"))!.Value);
    }

    [Fact]
    public async Task AValue_MayBeEmpty()
    {
        await Repository().SetText(Text("separator", "en", string.Empty));

        Assert.Equal(string.Empty, (await Repository().FindText("separator", "en"))!.Value);
    }

    [Fact]
    public async Task ABlankIdentifierOrCulture_IsRefused()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().SetText(Text("", "en", "x")));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().SetText(Text("person-title", " ", "x")));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().FindText("", "en"));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().FindText("person-title", ""));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().GetTextList(" "));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().GetTextListByIdentifier(""));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().ClearText("", "en", UserA));
        await Assert.ThrowsAsync<ArgumentException>(() => Repository().GetTextHistoryList("person-title", ""));
    }

    #endregion -- Setting and finding --

    #region -- Changing a text --

    [Fact]
    public async Task ANewValue_BecomesTheCurrentOne_AndTheOldRowMovesToTheHistory()
    {
        DataTextParameter first = await Repository().SetText(Text("person-title", "en", "Person", UserA, T0));
        DataTextParameter second = await Repository().SetText(Text("person-title", "en", "Human", UserB, T0.AddMinutes(5)));

        DataTextParameter current = (await Repository().FindText("person-title", "en"))!;
        Assert.Equal((second.ID, "Human", UserB), (current.ID, current.Value, current.CreatedByUserID));
        Assert.NotEqual(first.ID, second.ID);

        HistoryTextParameter old = Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
        Assert.Equal((first.ID, "Person", UserA, T0), (old.ID, old.Value, old.CreatedByUserID, old.TimeCreated));
        Assert.Equal((T0.AddMinutes(5), UserB), (old.TimeEnded, old.EndedByUserID));
    }

    [Fact]
    public async Task ACultureWrittenInAnotherCase_ReplacesTheValue_InsteadOfAddingOne()
    {
        await Repository().SetText(Text("person-title", "da-DK", "Person", time: T0));
        await Repository().SetText(Text("person-title", "da-dk", "Menneske", time: T0.AddMinutes(1)));

        DataTextParameter current = Assert.Single(await Repository().GetTextListByIdentifier("person-title"));
        Assert.Equal(("da-dk", "Menneske"), (current.Culture, current.Value));

        HistoryTextParameter old = Assert.Single(await Repository().GetTextHistoryList("person-title", "DA-DK"));
        Assert.Equal(("da-DK", "Person"), (old.Culture, old.Value));
    }

    [Fact]
    public async Task EveryChange_AddsOneRowToTheHistory_InTheOrderTheyEnded()
    {
        foreach ((string value, int minute) in new[] { ("a", 0), ("b", 1), ("c", 2), ("d", 3) })
        {
            await Repository().SetText(Text("person-title", "en", value, time: T0.AddMinutes(minute)));
        }

        Assert.Equal("d", (await Repository().FindText("person-title", "en"))!.Value);

        List<HistoryTextParameter> history = await Repository().GetTextHistoryList("person-title", "en");
        Assert.Equal(["a", "b", "c"], history.Select(x => x.Value));
        Assert.Equal([T0.AddMinutes(1), T0.AddMinutes(2), T0.AddMinutes(3)], history.Select(x => x.TimeEnded));
    }

    [Fact]
    public async Task AClearedText_HasNoValue_AndItsLastValueIsInTheHistory()
    {
        ManualTimeProvider time = new(T0.AddHours(2));
        DataTextParameter set = await Repository().SetText(Text("person-title", "da", "Person", time: T0));

        bool cleared = await Repository(time).ClearText("person-title", "DA", UserB);

        Assert.True(cleared);
        Assert.Null(await Repository().FindText("person-title", "da"));
        HistoryTextParameter old = Assert.Single(await Repository().GetTextHistoryList("person-title", "da"));
        Assert.Equal((set.ID, "Person", UserB, T0.AddHours(2)), (old.ID, old.Value, old.EndedByUserID, old.TimeEnded));
    }

    [Fact]
    public async Task ClearingATextWithoutAValue_ReturnsFalseAndChangesNothing()
    {
        await Repository().SetText(Text("person-title", "en", "Person"));

        Assert.False(await Repository().ClearText("nothing", "en", UserA));
        Assert.False(await Repository().ClearText("person-title", "da", UserA));

        Assert.NotNull(await Repository().FindText("person-title", "en"));
        Assert.Empty(await Repository().GetTextHistoryList("person-title", "en"));
    }

    [Fact]
    public async Task AClearedText_CanBeSetAgain()
    {
        await Repository().SetText(Text("person-title", "en", "Person"));
        await Repository().ClearText("person-title", "en", UserA);

        await Repository().SetText(Text("person-title", "en", "Human"));

        Assert.Equal("Human", (await Repository().FindText("person-title", "en"))!.Value);
        Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
    }

    #endregion -- Changing a text --

    #region -- Listing --

    [Fact]
    public async Task TheTextsOfACulture_AreListedByIdentifier_AndOnlyThatCulture()
    {
        await Repository().SetText(Text("c-title", "en", "C"));
        await Repository().SetText(Text("a-title", "en", "A"));
        await Repository().SetText(Text("b-title", "en", "B"));
        await Repository().SetText(Text("a-title", "da", "A (da)"));

        Assert.Equal(["a-title", "b-title", "c-title"], (await Repository().GetTextList("en")).Select(x => x.TextIdentifier));
        Assert.Equal(["a-title"], (await Repository().GetTextList("DA")).Select(x => x.TextIdentifier));
        Assert.Empty(await Repository().GetTextList("sv"));
    }

    [Fact]
    public async Task TheCulturesOfAText_AreListedByCulture()
    {
        await Repository().SetText(Text("a-title", "sv", "A (sv)"));
        await Repository().SetText(Text("a-title", "da", "A (da)"));
        await Repository().SetText(Text("a-title", "en", "A"));
        await Repository().SetText(Text("b-title", "en", "B"));

        Assert.Equal(["da", "en", "sv"], (await Repository().GetTextListByIdentifier("a-title")).Select(x => x.Culture));
        Assert.Empty(await Repository().GetTextListByIdentifier("nothing"));
    }

    #endregion -- Listing --

    #region -- All or nothing --

    [Fact]
    public async Task WhenTheHistoryCannotBeWritten_TheChangeDoesNotHappenAtAll()
    {
        DataTextParameter first = await Repository().SetText(Text("person-title", "en", "Person"));

        // A row with the ID of the one that would move is already in the history, so the move fails halfway through.
        await using (EntityManagementContext context = _db.CreateContext())
        {
            context.Add(first.ToHistory(UserA, T0));
            await context.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<DbUpdateException>(() => Repository().SetText(Text("person-title", "en", "Human")));

        DataTextParameter current = (await Repository().FindText("person-title", "en"))!;
        Assert.Equal((first.ID, "Person"), (current.ID, current.Value));
        Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
    }

    [Fact]
    public async Task TheDatabase_RefusesTwoCurrentValuesOfOneTextInOneCulture_WhateverTheCase()
    {
        await using EntityManagementContext context = _db.CreateContext();
        context.Add(Text("person-title", "da-DK", "Person"));
        context.Add(Text("person-title", "DA-dk", "Menneske"));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public void TheCultureColumn_IsLongEnoughForAnyLanguageTag()
    {
        Assert.True(DataTextParameter.CultureMaxLength >= "zh-Hans-CN-u-ca-chinese".Length);
    }

    #endregion -- All or nothing --
}

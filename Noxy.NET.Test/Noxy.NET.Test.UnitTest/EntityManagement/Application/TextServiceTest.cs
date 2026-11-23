// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the service calls are left on their default token.

#pragma warning disable xUnit1051

using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Persistence.Repositories;
using Noxy.NET.Test.EntityManagement.Definitions;
using Noxy.NET.Test.EntityManagement.Persistence;

namespace Noxy.NET.Test.EntityManagement.Application;

public class TextServiceTest : IDisposable
{
    private static readonly Guid UserA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid UserB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly DateTime T0 = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = new();
    private readonly ManualTimeProvider _time = new(T0);
    private readonly DefinitionRegistry _definition;
    private readonly DefinitionTextRegistry _defaults;

    public TextServiceTest()
    {
        // A line, a rich text and a text; English for all of them, and Danish for two of them.
        _definition = DefinitionBuilder.Create(new ActionDefinition(builder =>
        {
            builder.AddTextParameter("person-title", TextKindEnum.Line);
            builder.AddTextParameter("intro", TextKindEnum.RichText);
            builder.AddTextParameter("note", TextKindEnum.Text);
        }));

        _defaults = DefinitionTextBuilder.Create(_definition,
            new ActionTexts(builder =>
            {
                builder.AddText("person-title", "en", "Person");
                builder.AddText("intro", "en", "<p>Welcome</p>");
                builder.AddText("note", "en", "Line one\nLine two");
            }),
            new ActionTexts(builder =>
            {
                builder.AddText("person-title", "da", "Person (da)");
                builder.AddText("intro", "da", "<p>Velkommen</p>");
            }));
    }

    public void Dispose() => _db.Dispose();

    // A service on a new context each time, so what it reads is what is in the database.
    private TextService Service() => new(_definition, _defaults, new TextRepository(_db.CreateContext(), _time), _time);

    private TextRepository Repository() => new(_db.CreateContext(), _time);

    #region -- Getting a text --

    [Fact]
    public async Task AText_NobodyHasEdited_ShowsItsDefault()
    {
        Assert.Equal("Person", await Service().GetText("person-title", "en"));
        Assert.Equal("Person (da)", await Service().GetText("person-title", "da"));
    }

    [Fact]
    public async Task AnEdit_ComesBeforeTheDefault()
    {
        await Service().SetText("person-title", "en", "Human", UserA);

        Assert.Equal("Human", await Service().GetText("person-title", "en"));
        Assert.Equal("Person (da)", await Service().GetText("person-title", "da"));
    }

    [Fact]
    public async Task ADefaultThatIsNeverEdited_IsNotInTheDatabase_SoANewerDefaultReachesEveryone()
    {
        Assert.Empty(await Repository().GetTextList("en"));

        await Service().GetText("person-title", "en");
        await Service().GetTextMap("en");

        Assert.Empty(await Repository().GetTextList("en"));
    }

    [Fact]
    public async Task ACultureWithoutADefault_UsesItsLanguage_ThenTheFallback()
    {
        Assert.Equal("Person (da)", await Service().GetText("person-title", "da-DK"));
        Assert.Equal("<p>Velkommen</p>", await Service().GetText("intro", "da-DK"));

        // "note" has no Danish text at all, so English is shown.
        Assert.Equal("Line one\nLine two", await Service().GetText("note", "da-DK"));

        // Nothing at all in Swedish.
        Assert.Equal("Person", await Service().GetText("person-title", "sv-SE"));
    }

    [Fact]
    public async Task AnEditInTheCulture_BeatsAnEditInTheLanguage_WhichBeatsTheDefault()
    {
        await Service().SetText("person-title", "da", "Menneske", UserA);
        Assert.Equal("Menneske", await Service().GetText("person-title", "da-DK"));

        await Service().SetText("person-title", "da-DK", "Menneske (Danmark)", UserA);
        Assert.Equal("Menneske (Danmark)", await Service().GetText("person-title", "da-DK"));
        Assert.Equal("Menneske", await Service().GetText("person-title", "da"));
        Assert.Equal("Menneske", await Service().GetText("person-title", "da-SE"));
    }

    [Fact]
    public async Task TheDefaultOfTheCulture_BeatsAnEditOfTheFallback()
    {
        // English was edited, but Danish has a default of its own, and a Dane should see that.
        await Service().SetText("person-title", "en", "Human", UserA);

        Assert.Equal("Person (da)", await Service().GetText("person-title", "da"));
    }

    [Fact]
    public async Task AnEditOfTheFallback_IsUsedWhenTheCultureHasNothing()
    {
        await Service().SetText("note", "en", "Edited note", UserA);

        Assert.Equal("Edited note", await Service().GetText("note", "da"));
        Assert.Equal("Edited note", await Service().GetText("note", "sv"));
    }

    [Fact]
    public async Task ALanguageCanBeAddedWithoutADeployment_ByEditingATextInANewCulture()
    {
        await Service().SetText("person-title", "sv", "Person (sv)", UserA);

        Assert.Equal("Person (sv)", await Service().GetText("person-title", "sv"));
        Assert.Equal("Person (sv)", await Service().GetText("person-title", "sv-SE"));
        Assert.Equal("<p>Welcome</p>", await Service().GetText("intro", "sv"));
    }

    [Theory]
    [InlineData("da-DK", "Menneske")]
    [InlineData("da-dk", "Menneske")]
    [InlineData("DA-DK", "Menneske")]
    [InlineData("DA", "Person (da)")]
    [InlineData("EN", "Person")]
    public async Task TheCulture_IsComparedWithoutRegardToCase(string culture, string expected)
    {
        await Service().SetText("person-title", "da-DK", "Menneske", UserA);

        Assert.Equal(expected, await Service().GetText("person-title", culture));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("english")]
    [InlineData("en_US")]
    [InlineData("<script>alert(1)</script>")]
    public async Task AMissingOrInvalidCulture_GivesTheFallback_InsteadOfAnError(string? culture)
    {
        Assert.Equal("Person", await Service().GetText("person-title", culture));
        Assert.Equal("Person", (await Service().GetTextMap(culture))["person-title"]);
    }

    [Fact]
    public async Task ATextThatIsNotDefined_IsAMistakeOfTheCode()
    {
        KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().GetText("missing", "en"));

        Assert.Contains("'missing'", exception.Message);
    }

    #endregion -- Getting a text --

    #region -- Getting every text --

    [Fact]
    public async Task EveryText_IsGivenAtOnce_EachInTheBestCultureItHas()
    {
        IReadOnlyDictionary<string, string> map = await Service().GetTextMap("da-DK");

        Assert.Equal(3, map.Count);
        Assert.Equal("Person (da)", map["person-title"]);
        Assert.Equal("<p>Velkommen</p>", map["intro"]);
        Assert.Equal("Line one\nLine two", map["note"]);
    }

    [Fact]
    public async Task TheMapAndTheSingleText_AlwaysAgree()
    {
        await Service().SetText("person-title", "da", "Menneske", UserA);
        await Service().SetText("note", "en", "Edited note", UserA);
        await Service().SetText("intro", "da-DK", "<p>Goddag</p>", UserA);

        foreach (string culture in new[] { "en", "da", "da-DK", "da-SE", "sv", "zh-Hans-CN" })
        {
            IReadOnlyDictionary<string, string> map = await Service().GetTextMap(culture);
            foreach (string identifier in new[] { "person-title", "intro", "note" })
            {
                Assert.Equal(await Service().GetText(identifier, culture), map[identifier]);
            }
        }
    }

    [Fact]
    public async Task AnEditOfATextThatIsNoLongerDefined_IsLeftOutOfTheMap()
    {
        // A text parameter is never removed from a definition, but if it were, what is left in the database must not appear.
        await Repository().SetText(new() { TextIdentifier = "gone", Culture = "en", Value = "Old", CreatedByUserID = UserA });

        IReadOnlyDictionary<string, string> map = await Service().GetTextMap("en");

        Assert.DoesNotContain("gone", map.Keys);
        Assert.Equal(3, map.Count);
    }

    #endregion -- Getting every text --

    #region -- Editing a text --

    [Fact]
    public async Task AnEdit_IsStoredWithWhoMadeItAndWhen()
    {
        DataTextParameter edit = await Service().SetText("person-title", "en", "Human", UserA);

        DataTextParameter? stored = await Repository().FindText("person-title", "en");
        Assert.NotNull(stored);
        Assert.Equal((edit.ID, "Human", UserA, T0), (stored.ID, stored.Value, stored.CreatedByUserID, stored.TimeCreated));
    }

    [Fact]
    public async Task ASecondEdit_ReplacesTheFirst_AndTheFirstMovesToTheHistory()
    {
        await Service().SetText("person-title", "en", "Human", UserA);
        _time.Now = T0.AddMinutes(10);
        await Service().SetText("person-title", "en", "Individual", UserB);

        Assert.Equal("Individual", (await Repository().FindText("person-title", "en"))!.Value);

        HistoryTextParameter old = Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
        Assert.Equal(("Human", UserA, UserB, T0.AddMinutes(10)), (old.Value, old.CreatedByUserID, old.EndedByUserID, old.TimeEnded));
    }

    [Fact]
    public async Task SettingTheValueItAlreadyHas_ChangesNothing()
    {
        DataTextParameter first = await Service().SetText("person-title", "en", "Human", UserA);
        _time.Now = T0.AddMinutes(10);

        DataTextParameter again = await Service().SetText("person-title", "en", "Human", UserB);

        Assert.Equal(first.ID, again.ID);
        Assert.Empty(await Repository().GetTextHistoryList("person-title", "en"));
    }

    [Fact]
    public async Task ACultureWrittenInAnotherCase_ReplacesTheEdit_InsteadOfAddingOne()
    {
        await Service().SetText("person-title", "da-DK", "Menneske", UserA);
        await Service().SetText("person-title", "DA-dk", "Individ", UserA);

        Assert.Equal("Individ", (await Repository().GetTextListByIdentifier("person-title")).Single().Value);
        Assert.Single(await Repository().GetTextHistoryList("person-title", "da-DK"));
    }

    [Fact]
    public async Task ATextWithLineBreaks_IsFineForATextAndARichText_ButNotForALine()
    {
        await Service().SetText("note", "en", "One\r\nTwo", UserA);
        await Service().SetText("intro", "en", "<p>One</p>\n<p>Two</p>", UserA);

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => Service().SetText("person-title", "en", "One\nTwo", UserA));
        Assert.Contains("line break", exception.Message);
        await Assert.ThrowsAsync<ArgumentException>(() => Service().SetText("person-title", "en", "One\rTwo", UserA));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnEmptyText_IsRefused_ResetIsHowToGoBack(string value)
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => Service().SetText("person-title", "en", value, UserA));

        Assert.Contains("reset", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("english")]
    [InlineData("en_US")]
    [InlineData("da-")]
    public async Task ACultureThatIsNotALanguageTag_IsRefusedWhenEditing(string culture)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Service().SetText("person-title", culture, "Human", UserA));
    }

    [Fact]
    public async Task ATextThatIsTooLong_IsRefused()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Service().SetText("note", "en", new string('a', BaseData.TextMaxLength + 1), UserA));

        await Service().SetText("note", "en", new string('a', BaseData.TextMaxLength), UserA);
    }

    [Fact]
    public async Task EditingATextThatIsNotDefined_IsAMistakeOfTheCode()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().SetText("missing", "en", "x", UserA));

        Assert.Empty(await Repository().GetTextList("en"));
    }

    [Fact]
    public async Task ARefusedEdit_ChangesNothing()
    {
        await Service().SetText("person-title", "en", "Human", UserA);

        await Assert.ThrowsAsync<ArgumentException>(() => Service().SetText("person-title", "en", "One\nTwo", UserB));

        Assert.Equal("Human", await Service().GetText("person-title", "en"));
        Assert.Empty(await Repository().GetTextHistoryList("person-title", "en"));
    }

    #endregion -- Editing a text --

    #region -- Going back to the default --

    [Fact]
    public async Task AResetText_ShowsItsDefaultAgain_AndTheEditIsInTheHistory()
    {
        await Service().SetText("person-title", "en", "Human", UserA);
        _time.Now = T0.AddHours(1);

        bool reset = await Service().ResetText("person-title", "en", UserB);

        Assert.True(reset);
        Assert.Equal("Person", await Service().GetText("person-title", "en"));
        Assert.Empty(await Repository().GetTextList("en"));

        HistoryTextParameter old = Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
        Assert.Equal(("Human", UserA, UserB, T0.AddHours(1)), (old.Value, old.CreatedByUserID, old.EndedByUserID, old.TimeEnded));
    }

    [Fact]
    public async Task ResettingATextThatWasNotEdited_ReturnsFalse()
    {
        Assert.False(await Service().ResetText("person-title", "en", UserA));

        await Service().SetText("person-title", "en", "Human", UserA);
        Assert.False(await Service().ResetText("person-title", "da", UserA));
    }

    [Fact]
    public async Task ResettingATextTwice_OnlyEndsTheEditOnce()
    {
        await Service().SetText("person-title", "en", "Human", UserA);

        Assert.True(await Service().ResetText("person-title", "en", UserA));
        Assert.False(await Service().ResetText("person-title", "en", UserA));
        Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
    }

    [Fact]
    public async Task ResettingATextThatIsNotDefined_IsAMistakeOfTheCode()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().ResetText("missing", "en", UserA));
    }

    [Fact]
    public async Task AnEditedText_CanBeEditedAgainAfterTheReset()
    {
        await Service().SetText("person-title", "en", "Human", UserA);
        await Service().ResetText("person-title", "en", UserA);

        await Service().SetText("person-title", "en", "Individual", UserA);

        Assert.Equal("Individual", await Service().GetText("person-title", "en"));
        Assert.Single(await Repository().GetTextHistoryList("person-title", "en"));
    }

    #endregion -- Going back to the default --
}

using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Definitions.Models;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.Test.EntityManagement.Definitions;

public class DefinitionTextBuilderTest
{
    // A definition with two text parameters, which the texts are checked against.
    private static readonly DefinitionRegistry Definition = DefinitionBuilder.Create(new ActionDefinition(builder =>
    {
        builder.AddTextParameter("person-title", TextKindEnum.Line);
        builder.AddTextParameter("intro", TextKindEnum.RichText);
    }));

    private sealed class ActionTexts(Action<IDefinitionTextBuilder> define) : IDefinitionTexts
    {
        public void Define(IDefinitionTextBuilder builder) => define(builder);
    }

    private static ActionTexts English => new(builder =>
    {
        builder.AddText("person-title", "en", "Person");
        builder.AddText("intro", "en", "<p>Welcome</p>");
    });

    private static DefinitionException Fails(params IDefinitionTexts[] texts)
    {
        return Assert.Throws<DefinitionException>(() => DefinitionTextBuilder.Create(Definition, texts));
    }

    #region -- Valid texts --

    [Fact]
    public void TheTexts_AreBuilt_AndCanBeLookedUp()
    {
        DefinitionTextRegistry registry = DefinitionTextBuilder.Create(Definition, English);

        Assert.Equal("en", registry.FallbackCulture);
        Assert.Equal(["person-title", "intro"], registry.TextList.Select(x => x.TextIdentifier));
        Assert.Equal(["en"], registry.CultureList);

        Assert.True(registry.TryGetText("person-title", "en", out DefinitionText? text));
        Assert.Equal("Person", text.Value);
        Assert.False(registry.TryGetText("person-title", "da", out _));
        Assert.False(registry.TryGetText("nothing", "en", out _));
    }

    [Fact]
    public void ATextCanBeInSeveralLanguages_EachInItsOwnFileOrAllInOne()
    {
        ActionTexts danish = new(builder =>
        {
            builder.AddText("person-title", "da", "Person");
            builder.AddText("intro", "da", "<p>Velkommen</p>");
        });
        ActionTexts german = new(builder => builder.AddText("person-title", "de-DE", "Person"));

        DefinitionTextRegistry registry = DefinitionTextBuilder.Create(Definition, English, danish, german);

        Assert.Equal(["en", "da", "de-DE"], registry.CultureList);
        Assert.Equal(["person-title", "intro"], registry.GetTextList("da").Select(x => x.TextIdentifier));
        Assert.Equal(["person-title"], registry.GetTextList("de-DE").Select(x => x.TextIdentifier));
        Assert.Empty(registry.GetTextList("sv"));
    }

    [Fact]
    public void TheCulture_IsComparedWithoutRegardToCase()
    {
        ActionTexts danish = new(builder => builder.AddText("person-title", "da-DK", "Person"));

        DefinitionTextRegistry registry = DefinitionTextBuilder.Create(Definition, English, danish);

        Assert.True(registry.TryGetText("person-title", "DA-dk", out DefinitionText? text));
        Assert.Equal("da-DK", text.Culture);
        Assert.Equal(["person-title"], registry.GetTextList("da-dk").Select(x => x.TextIdentifier));
    }

    [Fact]
    public void ATextCanSayAnythingInTheLanguageItIsWrittenIn()
    {
        ActionTexts text = new(builder =>
        {
            builder.AddText("person-title", "en", "Person");
            builder.AddText("intro", "en", "Æble ø 🙂\r\n<b>fed</b>");
        });

        Assert.True(DefinitionTextBuilder.Create(Definition, text).TryGetText("intro", "en", out DefinitionText? intro));
        Assert.Equal("Æble ø 🙂\r\n<b>fed</b>", intro.Value);
    }

    [Fact]
    public void TheFallbackCulture_CanBeAnotherThanEnglish()
    {
        ActionTexts danish = new(builder =>
        {
            builder.AddText("person-title", "da", "Person");
            builder.AddText("intro", "da", "Velkommen");
        });

        DefinitionTextRegistry registry = DefinitionTextBuilder.Create(Definition, "da", danish);

        Assert.Equal("da", registry.FallbackCulture);
    }

    [Fact]
    public void TheCallsCanBeMadeInAnyOrder_AndInManyFiles()
    {
        ActionTexts first = new(builder => builder.AddText("intro", "en", "Welcome"));
        ActionTexts second = new(builder => builder.AddText("person-title", "en", "Person"));

        DefinitionTextRegistry registry = DefinitionTextBuilder.Create(Definition, first, second);

        Assert.Equal(["intro", "person-title"], registry.TextList.Select(x => x.TextIdentifier));
    }

    #endregion -- Valid texts --

    #region -- What is checked --

    [Fact]
    public void AText_OfATextParameterThatIsNotDefined_IsReported()
    {
        ActionTexts texts = new(builder =>
        {
            builder.AddText("person-title", "en", "Person");
            builder.AddText("intro", "en", "Welcome");
            builder.AddText("missing", "en", "Nobody asked for this");
        });

        Assert.Equal(["Text 'missing' (en): the text parameter is not defined."], Fails(texts).ErrorList);
    }

    [Fact]
    public void ATextParameterWithoutATextInTheFallbackCulture_IsReported()
    {
        ActionTexts texts = new(builder => builder.AddText("person-title", "en", "Person"));

        Assert.Equal(["Text parameter 'intro': there is no text in the fallback culture 'en'."], Fails(texts).ErrorList);
    }

    [Fact]
    public void ATextParameterThatOnlyHasAnotherLanguage_IsReported_ForTheFallbackCulture()
    {
        ActionTexts texts = new(builder =>
        {
            builder.AddText("person-title", "da", "Person");
            builder.AddText("intro", "da", "Velkommen");
        });

        Assert.Equal(
        [
            "Text parameter 'person-title': there is no text in the fallback culture 'en'.",
            "Text parameter 'intro': there is no text in the fallback culture 'en'."
        ], Fails(texts).ErrorList);
    }

    [Fact]
    public void NoTexts_AtAll_ReportsEveryTextParameter()
    {
        Assert.Equal(2, Fails().ErrorList.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("e")]
    [InlineData("english")]
    [InlineData("en_US")]
    [InlineData("en-")]
    [InlineData("-en")]
    [InlineData("en--US")]
    [InlineData("en US")]
    [InlineData("æø")]
    public void ACultureThatIsNotALanguageTag_IsReported(string culture)
    {
        ActionTexts texts = new(builder =>
        {
            builder.AddText("person-title", "en", "Person");
            builder.AddText("intro", "en", "Welcome");
            builder.AddText("intro", culture, "Other");
        });

        DefinitionException exception = Fails(texts);

        Assert.Contains(exception.ErrorList, x => x.Contains("the culture is not valid"));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("da-DK")]
    [InlineData("zh-Hans-CN")]
    [InlineData("sr-Latn-RS")]
    [InlineData("en-u-ca-gregory")]
    public void ALanguageTag_IsAValidCulture(string culture)
    {
        Assert.True(DefinitionCulture.IsValid(culture));
    }

    [Fact]
    public void ACulture_IsLimitedInLength()
    {
        // Thirty-five characters in parts of at most eight, and then one character more.
        Assert.Equal(DefinitionCulture.MaxLength, "en-aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaa".Length);
        Assert.True(DefinitionCulture.IsValid("en-aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaa"));
        Assert.False(DefinitionCulture.IsValid("en-aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaaa"));
    }

    [Fact]
    public void TheLongestCultureOfTheSchema_IsTheLongestOfTheData()
    {
        Assert.Equal(DefinitionCulture.MaxLength, DataTextParameter.CultureMaxLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AnEmptyText_IsReported(string value)
    {
        ActionTexts texts = new(builder =>
        {
            builder.AddText("person-title", "en", "Person");
            builder.AddText("intro", "en", "Welcome");
            builder.AddText("intro", "da", value);
        });

        Assert.Equal(["Text 'intro' (da): the text is empty."], Fails(texts).ErrorList);
    }

    [Fact]
    public void ATextTwiceInOneCulture_IsReported_WhateverTheCaseOfTheCulture()
    {
        ActionTexts texts = new(builder =>
        {
            builder.AddText("person-title", "da-DK", "Person");
            builder.AddText("person-title", "da-dk", "Menneske");
        });
        ActionTexts english = English;

        Assert.Equal(["Text 'person-title' (da-dk): defined more than once."], Fails(english, texts).ErrorList);
    }

    [Fact]
    public void TheSameTextTwiceInTwoFiles_IsAConflict_NotAnOverride()
    {
        ActionTexts first = English;
        ActionTexts second = new(builder => builder.AddText("person-title", "en", "Human"));

        Assert.Equal(["Text 'person-title' (en): defined more than once."], Fails(first, second).ErrorList);
    }

    [Fact]
    public void AFallbackCultureThatIsNotValid_IsReported()
    {
        DefinitionException exception = Assert.Throws<DefinitionException>(() => DefinitionTextBuilder.Create(Definition, "english", English));

        Assert.Equal(["The fallback culture 'english' is not valid."], exception.ErrorList);
    }

    [Fact]
    public void EveryProblemIsReportedTogether()
    {
        ActionTexts texts = new(builder =>
        {
            builder.AddText("missing", "en", "x");
            builder.AddText("intro", "bad culture", "");
        });

        // The text of a text parameter that is not defined, an invalid culture, an empty text, and both text parameters without a text in the fallback culture.
        Assert.Equal(5, Fails(texts).ErrorList.Count);
    }

    #endregion -- What is checked --

    #region -- The registry is read-only --

    [Fact]
    public void TheListsOfTheRegistry_CannotBeChanged()
    {
        DefinitionTextRegistry registry = DefinitionTextBuilder.Create(Definition, English);

        Assert.Throws<NotSupportedException>(() => ((IList<DefinitionText>)registry.TextList).Add(registry.TextList[0]));
        Assert.Throws<NotSupportedException>(() => ((IList<string>)registry.CultureList).Add("da"));
    }

    #endregion -- The registry is read-only --
}

using Noxy.NET.EntityManagement.Application;

namespace Noxy.NET.Test.EntityManagement.Application;

public class TextCultureTest
{
    [Theory]
    [InlineData("da-DK", "en", new[] { "da-DK", "da", "en" })]
    [InlineData("da", "en", new[] { "da", "en" })]
    [InlineData("en", "en", new[] { "en" })]
    [InlineData("en-GB", "en", new[] { "en-GB", "en" })]
    [InlineData("zh-Hans-CN", "en", new[] { "zh-Hans-CN", "zh-Hans", "zh", "en" })]
    [InlineData("da-DK", "da", new[] { "da-DK", "da" })]
    [InlineData("sv", "da", new[] { "sv", "da" })]
    [InlineData("DA-dk", "EN", new[] { "DA-dk", "DA", "EN" })]
    public void TheChain_GoesFromTheCultureToItsLanguageToTheFallback(string culture, string fallback, string[] expected)
    {
        Assert.Equal(expected, TextCulture.GetChain(culture, fallback));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("english")]
    [InlineData("en_US")]
    [InlineData("<script>")]
    [InlineData("da-DK-" + "aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaaaaa")]
    public void ACultureThatIsMissingOrNotALanguageTag_IsNotAnError_ItGivesTheFallback(string? culture)
    {
        Assert.Equal(["en"], TextCulture.GetChain(culture, "en"));
    }

    [Fact]
    public void TheFallback_IsNeverInTheChainTwice_WhateverTheCase()
    {
        IReadOnlyList<string> chain = TextCulture.GetChain("EN-us", "en");

        Assert.Equal(["EN-us", "EN"], chain);
    }
}

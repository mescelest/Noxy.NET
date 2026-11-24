using System.ComponentModel;
using System.Reflection;
using Noxy.NET.MediaPlayer.Presentation.Attributes;
using Noxy.NET.MediaPlayer.Presentation.Models.FormModel;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class DisplayTextAttributeTest
{
    private sealed class Sample
    {
        [DisplayText(nameof(PresentationTexts.FieldAlbumArtist))]
        public string? AlbumArtist { get; set; }

        [DisplayText("NoSuchText")]
        public string? Missing { get; set; }
    }

    private static DisplayNameAttribute DisplayNameOf(string property)
    {
        return typeof(Sample).GetProperty(property)?.GetCustomAttribute<DisplayNameAttribute>() ?? throw new InvalidOperationException($"{property} has no display name.");
    }

    [Fact]
    public void DisplayName_IsTheTextOfTheKey_AndIsFoundAsAPlainDisplayName()
    {
        Assert.Equal(PresentationTexts.FieldAlbumArtist, DisplayNameOf(nameof(Sample.AlbumArtist)).DisplayName);
        Assert.Equal("Album artist", DisplayNameOf(nameof(Sample.AlbumArtist)).DisplayName);
    }

    [Fact]
    public void DisplayName_OfAKeyThatHasNoText_Throws_SoAMistakeIsNotShownAsABlankLabel()
    {
        Assert.Throws<InvalidOperationException>(() => DisplayNameOf(nameof(Sample.Missing)).DisplayName);
    }

    [Fact]
    public void EveryFieldOfEveryFormModel_HasADisplayTextWithAnExistingText()
    {
        List<PropertyInfo> propertyList =
        [
            .. typeof(FormModelEditMedia).Assembly.GetTypes()
                .Where(x => x.Namespace == typeof(FormModelEditMedia).Namespace && x.IsClass)
                .SelectMany(x => x.GetProperties())
                .Where(x => x.GetCustomAttribute<DisplayNameAttribute>() is not null)
        ];

        Assert.NotEmpty(propertyList);
        Assert.All(propertyList, property =>
        {
            DisplayNameAttribute attribute = property.GetCustomAttribute<DisplayNameAttribute>()!;
            Assert.IsType<DisplayTextAttribute>(attribute);
            Assert.False(string.IsNullOrWhiteSpace(attribute.DisplayName));
        });
    }

    [Fact]
    public void NoFormModelFile_HoldsALiteralDisplayName()
    {
        string folder = Path.Combine(StyleFileFixture.RepositoryRoot(), "Noxy.NET.MediaPlayer", "Noxy.NET.MediaPlayer.Presentation", "Models", "FormModel");

        List<string> violationList = [.. Directory.EnumerateFiles(folder, "*.cs").Where(x => File.ReadAllText(x).Contains("[DisplayName(", StringComparison.Ordinal)).Select(Path.GetFileName)!];

        Assert.Empty(violationList);
    }
}

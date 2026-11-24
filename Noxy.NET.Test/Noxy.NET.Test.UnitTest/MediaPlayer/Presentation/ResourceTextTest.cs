using System.Text.RegularExpressions;
using System.Xml.Linq;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public partial class ResourceTextTest
{
    private const string AppProject = "Noxy.NET.MediaPlayer.App";
    private const string PresentationProject = "Noxy.NET.MediaPlayer.Presentation";

    [GeneratedRegex(@"public static string (?<property>\w+) \{\s+get \{\s+return ResourceManager\.GetString\(""(?<key>\w+)""")]
    private static partial Regex DesignerPropertyPattern();

    private static string PathOf(string project, string fileName)
    {
        return Path.Combine(StyleFileFixture.RepositoryRoot(), "Noxy.NET.MediaPlayer", project, "Resources", fileName);
    }

    private static List<(string Key, string Value)> ReadResxList(string path)
    {
        return [.. XDocument.Load(path).Root!.Elements("data").Select(x => (x.Attribute("name")!.Value, x.Element("value")!.Value))];
    }

    private static List<(string Property, string Key)> ReadDesignerList(string path)
    {
        return [.. DesignerPropertyPattern().Matches(File.ReadAllText(path)).Select(x => (x.Groups["property"].Value, x.Groups["key"].Value))];
    }

    public static TheoryData<string, string, string> ResourceList => new()
    {
        { AppProject, "AppTexts.resx", "AppTexts.Designer.cs" },
        { PresentationProject, "PresentationTexts.resx", "PresentationTexts.Designer.cs" }
    };

    [Theory]
    [MemberData(nameof(ResourceList))]
    public void EveryTextInTheResx_HasAPropertyInTheDesigner_AndTheOtherWayRound(string project, string resx, string designer)
    {
        List<string> keyList = [.. ReadResxList(PathOf(project, resx)).Select(x => x.Key)];
        List<string> propertyList = [.. ReadDesignerList(PathOf(project, designer)).Select(x => x.Property)];

        Assert.Empty(keyList.Except(propertyList));
        Assert.Empty(propertyList.Except(keyList));
    }

    [Theory]
    [MemberData(nameof(ResourceList))]
    public void EveryDesignerProperty_ReadsTheTextOfItsOwnName(string project, string resx, string designer)
    {
        Assert.All(ReadDesignerList(PathOf(project, designer)), x => Assert.Equal(x.Property, x.Key));
    }

    [Theory]
    [MemberData(nameof(ResourceList))]
    public void NoTextIsListedTwice_AndNoTextIsEmpty(string project, string resx, string designer)
    {
        List<(string Key, string Value)> list = ReadResxList(PathOf(project, resx));

        Assert.Equal(list.Count, list.Select(x => x.Key).Distinct().Count());
        Assert.DoesNotContain(list, x => string.IsNullOrWhiteSpace(x.Value));
    }

    [GeneratedRegex(@"\b(?<class>AppTexts|PresentationTexts)\.(?<key>[A-Z]\w+)")]
    private static partial Regex TextUsePattern();

    // Every text a source file names has to exist, since a razor file that names a missing one fails only when it is built.
    [Fact]
    public void EveryTextThatTheSourcesName_Exists()
    {
        string root = Path.Combine(StyleFileFixture.RepositoryRoot(), "Noxy.NET.MediaPlayer");
        Dictionary<string, HashSet<string>> existing = new()
        {
            ["AppTexts"] = [.. ReadResxList(PathOf(AppProject, "AppTexts.resx")).Select(x => x.Key)],
            ["PresentationTexts"] = [.. ReadResxList(PathOf(PresentationProject, "PresentationTexts.resx")).Select(x => x.Key)]
        };
        char separator = Path.DirectorySeparatorChar;

        List<string> missingList =
        [
            .. Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(".razor", StringComparison.Ordinal) || x.EndsWith(".cs", StringComparison.Ordinal))
                .Where(x => !x.Contains($"{separator}bin{separator}") && !x.Contains($"{separator}obj{separator}") && !x.EndsWith(".Designer.cs", StringComparison.Ordinal))
                .SelectMany(path => TextUsePattern().Matches(File.ReadAllText(path)).Select(match => (path, Class: match.Groups["class"].Value, Key: match.Groups["key"].Value)))
                .Where(x => !existing[x.Class].Contains(x.Key) && x.Key is not ("ResourceManager" or "Culture"))
                .Select(x => $"{Path.GetFileName(x.path)}: {x.Class}.{x.Key}")
                .Distinct()
        ];

        Assert.Empty(missingList);
    }
}

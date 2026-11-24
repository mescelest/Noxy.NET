using System.Text.RegularExpressions;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public static partial class MarkupFileFixture
{
    // What a screen reader or a tooltip reads, and what a component shows as its own words.
    private const string TextAttributePattern = "title|aria-label|placeholder|Title|Label|EmptyTitle|EmptyDescription|Description|Hint|alt";

    // A name of a page starts with a capital (Name="Library"); the name of an input is lowercase (Name="server") and is not text.
    [GeneratedRegex(@"[\s](?<attribute>(?:" + TextAttributePattern + @")|Name)=""(?<value>[^""@]*[A-Za-z]{2,}[^""@]*)""")]
    private static partial Regex TextAttribute();

    [GeneratedRegex(@">(?<text>[^<>@{}\r\n]*[A-Za-z]{2,}[^<>@{}\r\n]*)</")]
    private static partial Regex TextNode();

    // A line of markup that is only words, such as the label of a button written on its own line.
    [GeneratedRegex(@"^\s+(?<text>[A-Z][A-Za-z ,.'!?:-]{3,})$")]
    private static partial Regex TextLine();

    // Words that are not text for a person: an address the field starts with.
    private static readonly string[] AllowedList = ["https://"];

    // Every razor file of the App.
    public static List<string> RazorPathList()
    {
        string appFolder = Path.Combine(StyleFileFixture.RepositoryRoot(), "Noxy.NET.MediaPlayer", "Noxy.NET.MediaPlayer.App");
        char separator = Path.DirectorySeparatorChar;

        return
        [
            .. Directory.EnumerateFiles(appFolder, "*.razor", SearchOption.AllDirectories)
                .Where(x => !x.Contains($"{separator}bin{separator}") && !x.Contains($"{separator}obj{separator}"))
                .Order()
        ];
    }

    // The markup of a file: everything above its code block.
    private static string MarkupOf(string content)
    {
        int code = content.IndexOf("@code", StringComparison.Ordinal);

        return code < 0 ? content : content[..code];
    }

    // The literal words in the markup, each with its line, as found in a text node, a text attribute or a line of its own.
    public static List<string> TextLiteralList(string content)
    {
        List<string> result = [];
        string[] lineList = MarkupOf(content).Split('\n');

        for (int index = 0; index < lineList.Length; index++)
        {
            string line = lineList[index].TrimEnd('\r');
            if (line.TrimStart().StartsWith("@using", StringComparison.Ordinal) || line.TrimStart().StartsWith("@inject", StringComparison.Ordinal)) continue;

            foreach (Match match in TextNode().Matches(line)) result.Add($"line {index + 1}: {match.Groups["text"].Value.Trim()}");
            foreach (Match match in TextAttribute().Matches(line))
            {
                string value = match.Groups["value"].Value;
                if (AllowedList.Contains(value)) continue;
                if (match.Groups["attribute"].Value == "Name" && !char.IsUpper(value[0])) continue;

                result.Add($"line {index + 1}: {match.Groups["attribute"].Value}=\"{value}\"");
            }

            if (TextLine().Match(line) is { Success: true } plain) result.Add($"line {index + 1}: {plain.Groups["text"].Value.Trim()}");
        }

        return result;
    }
}

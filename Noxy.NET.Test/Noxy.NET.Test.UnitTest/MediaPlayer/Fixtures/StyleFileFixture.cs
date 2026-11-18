using System.Globalization;
using System.Text.RegularExpressions;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public static partial class StyleFileFixture
{
    private static readonly string[] AllowedColourWordList = ["transparent", "currentcolor", "inherit", "initial", "unset", "none"];

    private static readonly string[] ColourWordList =
    [
        "white", "black", "red", "green", "blue", "yellow", "orange", "purple", "pink", "gray", "grey", "silver", "gold", "navy", "teal", "maroon", "olive", "lime", "aqua", "fuchsia", "brown", "cyan", "magenta", "beige", "coral",
        "crimson", "indigo", "ivory", "khaki", "lavender", "salmon", "tan", "turquoise", "violet", "whitesmoke", "lightgray", "lightgrey", "darkgray", "darkgrey", "dimgray", "dimgrey"
    ];

    private static readonly string[] DurationPropertyList = ["transition", "animation", "transition-duration", "animation-duration", "transition-delay", "animation-delay"];

    // Every style file of the App that is not one of the two that may hold literals.
    public static List<string> ComponentStylePathList()
    {
        string appFolder = Path.Combine(RepositoryRoot(), "Noxy.NET.MediaPlayer", "Noxy.NET.MediaPlayer.App");
        char separator = Path.DirectorySeparatorChar;

        return Directory.EnumerateFiles(appFolder, "*.css", SearchOption.AllDirectories)
            .Where(x => !x.Contains($"{separator}bin{separator}") && !x.Contains($"{separator}obj{separator}") && !x.Contains($"{separator}lib{separator}"))
            .Where(x => Path.GetFileName(x) is not ("variables.css" or "breakpoints.css"))
            .Order()
            .ToList();
    }

    // A style file the App holds literals in on purpose, found by its name.
    public static string AppStylePath(string fileName)
    {
        string appFolder = Path.Combine(RepositoryRoot(), "Noxy.NET.MediaPlayer", "Noxy.NET.MediaPlayer.App");

        return Directory.EnumerateFiles(appFolder, fileName, SearchOption.AllDirectories).First(x => x.Contains($"{Path.DirectorySeparatorChar}wwwroot{Path.DirectorySeparatorChar}css{Path.DirectorySeparatorChar}"));
    }

    // The UI libraries the app uses (the others, such as the wizard and the graphs, are not referenced and may read variables the app does not define).
    private static readonly string[] UsedUiLibraryList = ["Shared", "Shared.Fluxor", "Elements", "Table", "Dialog", "WebForm", "Graphics"];

    public static List<string> UiStylePathList()
    {
        string uiFolder = Path.Combine(RepositoryRoot(), "Noxy.NET.UI");
        char separator = Path.DirectorySeparatorChar;

        return UsedUiLibraryList.SelectMany(library => Directory.EnumerateFiles(Path.Combine(uiFolder, $"Noxy.NET.UI.{library}"), "*.css", SearchOption.AllDirectories))
            .Where(x => !x.Contains($"{separator}bin{separator}") && !x.Contains($"{separator}obj{separator}"))
            .Order()
            .ToList();
    }

    public static string RepositoryRoot()
    {
        DirectoryInfo? folder = new(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "Noxy.NET.sln"))) folder = folder.Parent;

        return folder?.FullName ?? throw new InvalidOperationException("The solution folder was not found above the test's folder.");
    }

    // The names a style file reads with var(--name...).
    public static List<string> VariableReadList(string css)
    {
        return VariableReadRegex().Matches(StripComments(css)).Select(x => x.Groups[1].Value).Distinct().Order().ToList();
    }

    // The names a style file defines (a declaration of --name).
    public static List<string> VariableDefinitionList(string css)
    {
        return VariableDefinitionRegex().Matches(StripComments(css)).Select(x => x.Groups[1].Value).Distinct().Order().ToList();
    }

    public static List<string> ColourLiteralList(string css)
    {
        List<string> violationList = [];
        foreach ((string property, string value) in DeclarationList(css))
        {
            string bare = StripVariables(value);
            violationList.AddRange(HexColourRegex().Matches(bare).Select(x => $"{property}: {x.Value}"));
            violationList.AddRange(ColourFunctionRegex().Matches(bare).Select(x => $"{property}: {x.Value}"));
            violationList.AddRange(WordRegex().Matches(bare).Select(x => x.Value).Where(x => ColourWordList.Contains(x.ToLowerInvariant()) && !AllowedColourWordList.Contains(x.ToLowerInvariant())).Select(x => $"{property}: {x}"));
        }

        return violationList;
    }

    // A font-family (or the font shorthand) must be a variable; inherit is allowed.
    public static List<string> FontLiteralList(string css)
    {
        return DeclarationList(css)
            .Where(x => x.Property is "font-family" or "font")
            .Where(x => !x.Value.Trim().StartsWith("var(", StringComparison.Ordinal) && !x.Value.Trim().Equals("inherit", StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Property}: {x.Value.Trim()}")
            .ToList();
    }

    public static List<string> DurationLiteralList(string css)
    {
        return DeclarationList(css)
            .Where(x => DurationPropertyList.Contains(x.Property))
            .SelectMany(x => DurationRegex().Matches(StripVariables(x.Value)).Select(match => $"{x.Property}: {match.Value}"))
            .ToList();
    }

    // A size in px, rem or em other than zero (a media query condition counts: those belong in breakpoints.css).
    public static List<string> SizeLiteralList(string css)
    {
        List<string> violationList = [];
        foreach ((string property, string value) in DeclarationList(css))
        {
            foreach (Match match in SizeRegex().Matches(StripVariables(value)))
            {
                if (double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) != 0) violationList.Add($"{property}: {match.Value}");
            }
        }

        return violationList;
    }

    private static List<(string Property, string Value)> DeclarationList(string css)
    {
        return DeclarationRegex().Matches(StripComments(css)).Select(x => (x.Groups[1].Value.Trim().ToLowerInvariant(), x.Groups[2].Value)).ToList();
    }

    private static string StripComments(string css)
    {
        return CommentRegex().Replace(css, string.Empty);
    }

    // What is inside var(...) is a fallback or a name, not a literal of this file.
    private static string StripVariables(string value)
    {
        string result = value;
        Match match = VariableCallRegex().Match(result);
        while (match.Success)
        {
            result = VariableCallRegex().Replace(result, string.Empty);
            match = VariableCallRegex().Match(result);
        }

        return result;
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"([a-zA-Z-]+)\s*:\s*([^;{}]+)")]
    private static partial Regex DeclarationRegex();

    [GeneratedRegex(@"var\([^()]*\)")]
    private static partial Regex VariableCallRegex();

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b")]
    private static partial Regex HexColourRegex();

    [GeneratedRegex(@"\b(?:rgb|rgba|hsl|hsla|hwb|lab|lch|oklab|oklch)\(")]
    private static partial Regex ColourFunctionRegex();

    [GeneratedRegex(@"[a-zA-Z]+")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"\d*\.?\d+m?s\b")]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"(\d*\.?\d+)(?:px|rem|em)\b")]
    private static partial Regex SizeRegex();

    [GeneratedRegex(@"var\(\s*(--[a-zA-Z0-9-]+)")]
    private static partial Regex VariableReadRegex();

    [GeneratedRegex(@"(?<![a-zA-Z0-9-])(--[a-zA-Z0-9-]+)\s*:")]
    private static partial Regex VariableDefinitionRegex();
}

using System.Text;
using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;

namespace LewdFilter.Domain.Services;

public class FilterCompilerService
{
    private static string Indent(int level) => new(' ', level * 4);

    public static string Compile(Filter filter)
    {
        StringBuilder sb = new();

        sb.AppendLine($"# {filter.Name}");
        sb.AppendLine();
        WriteCustomColors(filter, sb);

        foreach (FilterGroup group in filter.GroupList)
        {
            if (!string.IsNullOrWhiteSpace(group.Name))
            {
                sb.AppendLine($"## {group.Name}");
                sb.AppendLine();
            }

            foreach (FilterBlock block in group.BlockList)
            {
                sb.AppendLine(block.IsShow ? "Show" : "Hide");

                if (block.BaseTypeList.Count > 0)
                {
                    string bases = string.Join(" ", block.BaseTypeList.Select(b => $"\"{b}\""));
                    sb.AppendLine($"{Indent(1)}BaseType {bases}");
                }

                // Conditionals
                if (block.Rarity != null) sb.AppendLine($"{Indent(1)}Rarity {block.Rarity}");
                if (block.StackSize != null) sb.AppendLine($"{Indent(1)}StackSize {block.StackSize}");
                if (block.Quality != null) sb.AppendLine($"{Indent(1)}Quality {block.Quality}");
                if (block.UnidentifiedItemTier != null) sb.AppendLine($"{Indent(1)}UnidentifiedItemTier {block.UnidentifiedItemTier}");

                // Direct object rendering from our direct memory references
                if (block.TextColor != null) sb.AppendLine($"{Indent(1)}SetTextColor {block.TextColor}");
                if (block.BorderColor != null) sb.AppendLine($"{Indent(1)}SetBorderColor {block.BorderColor}");
                if (block.BackgroundColor != null) sb.AppendLine($"{Indent(1)}SetBackgroundColor {block.BackgroundColor}");

                if (block.FontSize.HasValue) sb.AppendLine($"{Indent(1)}SetFontSize {block.FontSize.Value}");

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static void WriteCustomColors(Filter filter, StringBuilder sb)
    {
        bool hasColors = filter.CustomColorCollection.Any(kvp => kvp.Value.Count > 0);

        if (!hasColors)
            return;

        sb.AppendLine("## Custom Colors");

        foreach ((ColorTypeEnum type, List<FilterColor>? list) in filter.CustomColorCollection)
        {
            if (list.Count == 0) continue;

            sb.AppendLine($"### {type}");
            sb.AppendLine($"#");

            foreach (FilterColor color in list)
            {
                sb.AppendLine($"# {color.Name}: {color} ({color.ToHex()})");
            }

            sb.AppendLine($"#");
        }
    }
}

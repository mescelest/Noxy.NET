using System.Text;
using LewdFilter.Domain.Enums;
using LewdFilter.Domain.Models;

namespace LewdFilter.Domain.Services;

public class FilterCompilerService
{
    private static string Indent(int level) => new(' ', level * 4);

    public string Compile(Filter filter)
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

                if (block.ClassList.Count > 0)
                {
                    string classes = string.Join(" ", block.ClassList.Select(c => $"\"{c}\""));
                    sb.AppendLine($"{Indent(1)}Class == {classes}");
                }

                if (block.BaseTypeList.Count > 0)
                {
                    string bases = string.Join(" ", block.BaseTypeList.Select(b => $"\"{b}\""));
                    sb.AppendLine($"{Indent(1)}BaseType == {bases}");
                }

                foreach (FilterRule rule in block.RuleList)
                {
                    string ruleLine = rule.ToFilterLine();
                    if (!string.IsNullOrWhiteSpace(ruleLine))
                    {
                        sb.AppendLine($"{Indent(1)}{ruleLine}");
                    }
                }

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

        foreach ((FilterColorTypeEnum type, IReadOnlyList<FilterColor> list) in filter.CustomColorCollection)
        {
            if (list.Count == 0) continue;

            sb.AppendLine($"### {type}");
            sb.AppendLine("#");

            foreach (FilterColor color in list)
            {
                sb.AppendLine($"# {color.Name}: {color} ({color.ToHex()})");
            }

            sb.AppendLine("#");
        }
    }
}

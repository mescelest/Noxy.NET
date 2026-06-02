using System.Text;
using LewdFilter.Domain.Models;

namespace LewdFilter.Domain.Services;

public class FilterCompilerService
{
    public string Compile(Filter filter)
    {
        StringBuilder sb = new();

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
                    sb.AppendLine($"    BaseType {bases}");
                }

                // Conditionals
                if (block.Rarity != null) sb.AppendLine($"    Rarity {block.Rarity}");
                if (block.StackSize != null) sb.AppendLine($"    StackSize {block.StackSize}");
                if (block.Quality != null) sb.AppendLine($"    Quality {block.Quality}");
                if (block.UnidentifiedItemTier != null) sb.AppendLine($"    UnidentifiedItemTier {block.UnidentifiedItemTier}");

                // Direct object rendering from our direct memory references
                if (block.TextColor != null) sb.AppendLine($"    SetTextColor {block.TextColor}");
                if (block.BorderColor != null) sb.AppendLine($"    SetBorderColor {block.BorderColor}");
                if (block.BackgroundColor != null) sb.AppendLine($"    SetBackgroundColor {block.BackgroundColor}");

                if (block.FontSize.HasValue) sb.AppendLine($"    SetFontSize {block.FontSize.Value}");

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}

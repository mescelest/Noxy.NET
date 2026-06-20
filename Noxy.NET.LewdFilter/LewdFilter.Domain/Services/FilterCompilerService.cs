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
                    sb.AppendLine($"{Indent(1)}Class {classes}");
                }

                if (block.BaseTypeList.Count > 0)
                {
                    string bases = string.Join(" ", block.BaseTypeList.Select(b => $"\"{b}\""));
                    sb.AppendLine($"{Indent(1)}BaseType {bases}");
                }

                if (block.Rarity != null) sb.AppendLine($"{Indent(1)}Rarity {block.Rarity}");
                if (block.ItemLevel != null) sb.AppendLine($"{Indent(1)}ItemLevel {block.ItemLevel}");
                if (block.AreaLevel != null) sb.AppendLine($"{Indent(1)}AreaLevel {block.AreaLevel}");
                if (block.Sockets != null) sb.AppendLine($"{Indent(1)}Sockets {block.Sockets}");
                if (block.Quality != null) sb.AppendLine($"{Indent(1)}Quality {block.Quality}");
                if (block.StackSize != null) sb.AppendLine($"{Indent(1)}StackSize {block.StackSize}");
                if (block.UnidentifiedItemTier != null) sb.AppendLine($"{Indent(1)}UnidentifiedItemTier {block.UnidentifiedItemTier}");
                if (block.WaystoneTier != null) sb.AppendLine($"{Indent(1)}WaystoneTier {block.WaystoneTier}");
                if (block.GemLevel != null) sb.AppendLine($"{Indent(1)}GemLevel {block.GemLevel}");

                if (block.BaseArmour != null) sb.AppendLine($"{Indent(1)}BaseArmour {block.BaseArmour}");
                if (block.BaseEvasion != null) sb.AppendLine($"{Indent(1)}BaseEvasion {block.BaseEvasion}");
                if (block.BaseEnergyShield != null) sb.AppendLine($"{Indent(1)}BaseEnergyShield {block.BaseEnergyShield}");

                if (block.Corrupted.HasValue) sb.AppendLine($"{Indent(1)}Corrupted {block.Corrupted.Value.ToString().ToLower()}");
                if (block.CorruptedMods != null) sb.AppendLine($"{Indent(1)}CorruptedMods {block.CorruptedMods}");
                if (block.AlwaysShow.HasValue) sb.AppendLine($"{Indent(1)}AlwaysShow {block.AlwaysShow.Value.ToString().ToLower()}");

                if (block.TextColor != null) sb.AppendLine($"{Indent(1)}SetTextColor {block.TextColor}");
                if (block.BorderColor != null) sb.AppendLine($"{Indent(1)}SetBorderColor {block.BorderColor}");
                if (block.BackgroundColor != null) sb.AppendLine($"{Indent(1)}SetBackgroundColor {block.BackgroundColor}");

                if (block.FontSize.HasValue) sb.AppendLine($"{Indent(1)}SetFontSize {block.FontSize.Value}");

                if (block.PlayEffect != null)
                {
                    string temp = block.PlayEffect.IsTemporary ? " Temp" : string.Empty;
                    sb.AppendLine($"{Indent(1)}PlayEffect {block.PlayEffect.Color}{temp}");
                }

                if (block.MinimapIcon != null)
                {
                    sb.AppendLine($"{Indent(1)}MinimapIcon {block.MinimapIcon.Size} {block.MinimapIcon.Color} {block.MinimapIcon.Shape}");
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

        foreach ((FilterColorTypeEnum type, List<FilterColor> list) in filter.CustomColorCollection)
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

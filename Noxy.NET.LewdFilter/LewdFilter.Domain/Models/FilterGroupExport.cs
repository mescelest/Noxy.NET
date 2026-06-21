using System.Text.Json.Serialization;

namespace LewdFilter.Domain.Models;

public record FilterGroupExport
{
    [JsonPropertyOrder(1)]
    public List<FilterColorExport> ColorList { get; init; } = [];

    [JsonPropertyOrder(2)]
    public FilterGroup Group { get; init; } = FilterGroup.Default;

    public FilterGroupExport(FilterGroup group, List<FilterColorExport> colorList)
    {
        Group = group;
        ColorList = colorList;
    }

    [JsonConstructor]
    public FilterGroupExport() { }
}

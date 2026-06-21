using System.Text.Json.Serialization;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterColorExport
{
    [JsonPropertyOrder(1)]
    public FilterColorTypeEnum Type { get; init; } = FilterColorTypeEnum.Background;

    [JsonPropertyOrder(2)]
    public FilterColor Color { get; init; } = FilterColor.DefaultBackground;

    public FilterColorExport(FilterColorTypeEnum type, FilterColor color)
    {
        Type = type;
        Color = color;
    }

    [JsonConstructor]
    public FilterColorExport() { }
}

using System.Diagnostics.CodeAnalysis;
using LewdFilter.Domain.Abstractions;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record Filter : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public List<FilterGroup> GroupList { get; set; } = [];
    public Dictionary<FilterColorTypeEnum, List<FilterColor>> CustomColorCollection { get; set; } = new()
    {
        { FilterColorTypeEnum.Text, [] },
        { FilterColorTypeEnum.Border, [] },
        { FilterColorTypeEnum.Background, [] }
    };

    protected Filter(Filter other) : base(other)
    {
        Name = other.Name;
        GroupList = other.GroupList.Select(g => g with { }).ToList();
        CustomColorCollection = other.CustomColorCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(c => c with { }).ToList());
    }

    public List<FilterColor> GetColorList(FilterColorTypeEnum type)
    {
        return CustomColorCollection.GetValueOrDefault(type, []);
    }

    public bool TryGetColor(FilterColorTypeEnum type, string? idString, [NotNullWhen(true)] out FilterColor? color)
    {
        color = Guid.TryParse(idString, out Guid targetId) ? GetColorList(type).FirstOrDefault(c => c.ID == targetId) : null;
        return color != null;
    }

    public bool TryGetGroup(Guid groupId, [NotNullWhen(true)] out FilterGroup? group)
    {
        group = GroupList.FirstOrDefault(g => g.ID == groupId);
        return group != null;
    }

    public bool TryGetBlock(Guid groupId, Guid blockId, [NotNullWhen(true)] out FilterGroup? group, [NotNullWhen(true)] out FilterBlock? block)
    {
        block = TryGetGroup(groupId, out group) ? group?.BlockList.FirstOrDefault(b => b.ID == blockId) : null;
        return block != null;
    }
}

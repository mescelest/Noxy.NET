using System.Diagnostics.CodeAnalysis;
using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public class Filter : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public List<FilterGroup> GroupList { get; set; } = [];
    public Dictionary<ColorTypeEnum, List<FilterColor>> CustomColorCollection { get; set; } = new()
    {
        { ColorTypeEnum.Text, [] },
        { ColorTypeEnum.Border, [] },
        { ColorTypeEnum.Background, [] }
    };

    public Filter Clone(Guid? newID = null)
    {
        return new()
        {
            ID = newID ?? ID,
            Name = Name,
            CustomColorCollection = CustomColorCollection,
            GroupList = GroupList
        };
    }

    public List<FilterColor> GetColorList(ColorTypeEnum type)
    {
        return CustomColorCollection.GetValueOrDefault(type, []);
    }

    public bool TryGetColor(ColorTypeEnum type, string? idString, [NotNullWhen(true)] out FilterColor? color)
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

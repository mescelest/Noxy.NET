using System.Diagnostics.CodeAnalysis;

namespace LewdFilter.Domain.Models;

public class FilterGroup : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public List<FilterBlock> BlockList { get; set; } = [];

    public static FilterGroup Default => new() { Name = "New group", BlockList = [FilterBlock.Default] };

    public FilterGroup Clone(Guid? targetID = null) => new()
    {
        ID = targetID ?? ID,
        Name = Name,
        BlockList = BlockList
    };

    public List<FilterBlock> AddBlock(FilterBlock newBlock)
    {
        return [.. BlockList, newBlock];
    }

    public List<FilterBlock> RemoveBlock(Guid blockId)
    {
        return BlockList.Where(b => b.ID != blockId).ToList();
    }

    public bool TryReplaceBlock(FilterBlock block, [NotNullWhen(true)] out List<FilterBlock>? result)
    {
        result = null;
        int index = BlockList.FindIndex(b => b.ID == block.ID);
        if (index == -1)
        {
            return false;
        }

        result = [.. BlockList];
        result[index] = block;
        return true;
    }
}

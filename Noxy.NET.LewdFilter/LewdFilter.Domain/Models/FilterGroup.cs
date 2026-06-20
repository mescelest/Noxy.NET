using System.Diagnostics.CodeAnalysis;
using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterGroup : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public List<FilterBlock> BlockList { get; set; } = [];

    public static FilterGroup Default => new() { Name = "New group", BlockList = [FilterBlock.Default] };

    public FilterGroup()
    {
    }

    protected FilterGroup(FilterGroup other) : base(other)
    {
        Name = other.Name;
        BlockList = other.BlockList.Select(b => b with { }).ToList();
    }

    public string ToGroupID()
    {
        return $"group-{ID}";
    }

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

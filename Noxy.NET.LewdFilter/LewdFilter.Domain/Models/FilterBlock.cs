using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterBlock : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsShow { get; set; } = true;

    public HashSet<string> ClassList { get; } = [];
    public HashSet<string> BaseTypeList { get; init; } = [];
    public IReadOnlyList<FilterRule> RuleList { get; init; } = [];

    public static FilterBlock Default => new() { Name = "New block" };

    protected FilterBlock(FilterBlock other) : base(other)
    {
        Name = other.Name;
        IsShow = other.IsShow;
        ClassList = [.. other.ClassList];
        BaseTypeList = [.. other.BaseTypeList];
        RuleList = other.RuleList.Select(r => r with { }).ToList();
    }

    public List<FilterRule> MoveRule(Guid id, int indexNew)
    {
        List<FilterRule> list = RuleList.ToList();
        int indexOld = list.FindIndex(r => r.ID == id);
        if (indexOld == -1 || indexNew < 0 || indexNew >= list.Count || indexOld == indexNew) return list;

        FilterRule item = list[indexOld];
        list.RemoveAt(indexOld);
        list.Insert(indexNew, item);
        return list;
    }

    public List<FilterRule> RemoveRule(Guid ruleId)
    {
        return RuleList.Where(r => r.ID != ruleId).ToList();
    }
}

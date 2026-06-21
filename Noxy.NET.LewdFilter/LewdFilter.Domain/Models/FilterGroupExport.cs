namespace LewdFilter.Domain.Models;

public record FilterGroupExport(FilterGroup Group, List<FilterColorExport> ColorList);

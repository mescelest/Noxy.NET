using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Application.Models;

public class FilterSchemaList
{
    public string? Search { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required string SortColumn { get; set; }
    public required ListSortDirection SortDirection { get; set; }
}

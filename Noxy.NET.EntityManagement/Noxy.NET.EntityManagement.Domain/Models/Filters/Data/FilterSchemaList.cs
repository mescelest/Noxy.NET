using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Models;

public class FilterDataParameterList
{
    public string? Search { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required string SortColumn { get; set; }
    public required ListSortDirection SortDirection { get; set; }
}

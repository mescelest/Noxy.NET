using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Model;

public record SortOption(string Column, ListSortDirection Direction, string Label);

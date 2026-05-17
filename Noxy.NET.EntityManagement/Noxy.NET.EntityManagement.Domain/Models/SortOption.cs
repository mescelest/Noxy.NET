using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Domain.Models;

public record SortOption(string Column, ListSortDirection Direction, string Label);

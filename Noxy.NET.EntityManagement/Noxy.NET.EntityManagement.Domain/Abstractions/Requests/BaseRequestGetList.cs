using System.ComponentModel;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Constants;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequestGetList<TResponse> : BaseRequestGet<TResponse>
{
    [JsonIgnore]
    public const int DefaultPageSize = 10;

    [DisplayName(TextConstants.LabelFormIsActivated)]
    [Description(TextConstants.HelpFormIsActivated)]
    public bool? IsActivated { get; set; }

    [DisplayName(TextConstants.LabelFormSearch)]
    [Description(TextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    [DisplayName(TextConstants.LabelFormPageSize)]
    [Description(TextConstants.HelpFormPageSize)]
    public int? PageSize { get; set; }

    [DisplayName(TextConstants.LabelFormPageNumber)]
    [Description(TextConstants.HelpFormPageNumber)]
    public int? PageNumber { get; set; }

    [DisplayName(TextConstants.LabelFormSortColumn)]
    [Description(TextConstants.HelpFormSortColumn)]
    public string? SortColumn { get; set; }

    [DisplayName(TextConstants.LabelFormSortDirection)]
    [Description(TextConstants.HelpFormSortDirection)]
    public ListSortDirection? SortDirection { get; set; }

    [JsonIgnore]
    [DisplayName(TextConstants.LabelFormSortColumn)]
    [Description(TextConstants.HelpFormSortColumn)]
    public (string Column, ListSortDirection Direction)? Sorting
    {
        get => string.IsNullOrWhiteSpace(SortColumn) || !SortDirection.HasValue ? null : (SortColumn, SortDirection.Value);
        set
        {
            SortColumn = value?.Column;
            SortDirection = value?.Direction;
        }
    }
}

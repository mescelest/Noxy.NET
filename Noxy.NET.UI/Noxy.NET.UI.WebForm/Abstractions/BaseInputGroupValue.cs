using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputGroupValue<TValue> : BaseInputValue<TValue>
{
    [Parameter]
    public string? DisplayName { get; set; }
    protected string DisplayNameCurrent => DisplayName ?? GetField()?.DisplayName ?? string.Empty;

    [Parameter]
    public string? Description { get; set; }
    protected string DescriptionCurrent => Description ?? GetField()?.Description ?? string.Empty;

    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }

    protected IReadOnlyList<string>? ErrorList => GetField()?.ErrorList;
}

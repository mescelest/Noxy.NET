using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputGroupValue<TValue> : BaseInputValue<TValue>
{
    [Parameter]
    public string? DisplayName { get; set; }
    protected string DisplayNameCurrent => DisplayName ?? Context?.GetField(ValueExpression)?.GetDisplayName() ?? string.Empty;

    [Parameter]
    public string? Description { get; set; }
    protected string DescriptionCurrent => Description ?? Context?.GetField(ValueExpression)?.GetDescription() ?? string.Empty;

    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }

    protected string[]? ErrorList => Context?.GetField(ValueExpression)?.GetErrorList();
}

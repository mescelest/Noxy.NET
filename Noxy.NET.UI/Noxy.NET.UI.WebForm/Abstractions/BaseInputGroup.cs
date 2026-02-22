using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputGroup<TValue> : BaseInput<TValue>
{
    [Parameter]
    public string? DisplayName { get; set; }
    protected string DisplayNameCurrent => DisplayName ?? DisplayNameContext ?? string.Empty;
    private string? DisplayNameContext => Context != null && Context.TryGetFieldDisplayName(GetName(), out string? value) ? value : null;

    [Parameter]
    public string? Description { get; set; }
    protected string DescriptionCurrent => Description ?? DescriptionContext ?? string.Empty;
    private string? DescriptionContext => Context != null && Context.TryGetFieldDescription(GetName(), out string? value) ? value : null;

    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }

    protected IReadOnlyList<string>? ErrorList => Context != null && Context.TryGetFieldErrorList(GetName(), out IReadOnlyList<string> list) ? list : null;
}

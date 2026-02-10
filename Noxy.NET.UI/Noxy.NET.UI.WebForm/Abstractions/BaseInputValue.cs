using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputValue<TValue> : BaseInput
{
    [Parameter, EditorRequired]
    public required TValue Value { get; set; }

    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    protected void NotifyChange(TValue value)
    {
        Context?.NotifyFieldChanged(GetName());
        ValueChanged.InvokeAsync(value);
    }

    protected string GetName()
    {
        return Name ?? GetFieldNameFromExpression(ValueExpression) ?? UUIDCode;
    }
}

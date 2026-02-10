using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;

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
        GetField()?.NotifyChange();
        ValueChanged.InvokeAsync(value);
    }

    protected IWebFormFieldContext? GetField()
    {
        return !string.IsNullOrWhiteSpace(Name)
            ? GetField(Name)
            : ValueExpression != null
                ? GetField(ValueExpression)
                : null;
    }
}

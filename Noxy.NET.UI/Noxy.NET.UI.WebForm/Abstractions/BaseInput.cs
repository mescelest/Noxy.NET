using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInput : ElementComponent
{
    [CascadingParameter]
    public IWebFormContext? Context { get; set; }

    [Parameter]
    public string? ID { get; set; }
    protected string IDCurrent => ID ?? UUIDCode;

    [Parameter]
    public string? Name { get; set; }

    protected IWebFormFieldContext? GetField(string name)
    {
        return Context?.GetField(name);
    }

    protected IWebFormFieldContext? GetField<T>(Expression<Func<T>>? expression)
    {
        return Context?.GetField(expression);
    }
}

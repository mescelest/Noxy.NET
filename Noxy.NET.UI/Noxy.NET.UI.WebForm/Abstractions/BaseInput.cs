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

    protected static string? GetFieldNameFromExpression<T>(Expression<Func<T>>? expression)
    {
        return expression?.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression m } => m.Member.Name,
            _ => null
        };
    }
}

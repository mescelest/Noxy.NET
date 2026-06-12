using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Interfaces;

public interface IBaseInput
{
    IWebFormContext? Context { get; set; }
    string? ID { get; set; }
    string? Name { get; set; }
}

public interface IBaseInput<TValue>
{
    TValue? Value { get; set; }
    EventCallback<TValue?> ValueChanged { get; set; }
    Expression<Func<TValue?>>? ValueExpression { get; set; }
}

using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Models;

public record InputElementCheckboxScope<TOption>(string ID, string Name, bool Checked, TOption Option, TOption? Value, EventCallback<TOption?> ValueChanged, Expression<Func<TOption?>>? ValueExpression);

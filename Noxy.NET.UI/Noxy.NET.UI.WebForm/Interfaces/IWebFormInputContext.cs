using System.Linq.Expressions;

namespace Noxy.NET.UI.Interfaces;

public interface IWebFormInputContext
{
    IWebFormFieldContext? GetField<T>(Expression<Func<T>>? expression);
    IWebFormFieldContext? GetField(string name);
}

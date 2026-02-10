using System.Linq.Expressions;

namespace Noxy.NET.UI.Interfaces;

public interface IWebFormContext<out TModel> : IWebFormContext
{
    TModel Model { get; }

    event Action<IWebFormContext<TModel>>? Changed;
}

public interface IWebFormContext
{
    bool HasError { get; }
    bool HasChanged { get; }

    IReadOnlySet<string> FieldNameList { get; }

    void RegisterField(string name);
    IWebFormFieldContext GetField<T>(Expression<Func<T>>? expression);
    IWebFormFieldContext GetField(string name);
    IWebFormFieldContext? TryGetField<T>(Expression<Func<T>>? expression);
    IWebFormFieldContext? TryGetField(string name);
    IReadOnlyList<string> GetFormErrorList();
    IReadOnlyList<string> GetFieldErrorList();
    IReadOnlyList<string> GetFieldErrorList(string name);
    bool TryGetFieldErrorList(string name, out IReadOnlyList<string> errors);
    void ClearErrorList();
    void Reset();
    bool Validate();
    bool ValidateField(string name);
    bool TryValidateField(string name, out bool result);
    void WriteError(string message);

    void HandleException(Exception exception);
    void HandleException(string message, Dictionary<string, IEnumerable<string>>? data = null);
}

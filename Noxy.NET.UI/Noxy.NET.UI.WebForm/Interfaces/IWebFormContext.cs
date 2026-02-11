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
    bool HasAnyError { get; }
    bool HasAnyChanges { get; }

    IReadOnlySet<string> FieldNameList { get; }

    void RegisterField(string name);
    bool TryRegisterField(string name);

    public void NotifyFieldChanged(string name);
    public string GetFieldDisplayName(string name);
    public bool TryGetFieldDisplayName(string name, out string? value);
    public string GetFieldDescription(string name);
    public bool TryGetFieldDescription(string name, out string? value);

    IReadOnlyList<string> GetFormErrorList();
    IReadOnlyList<string> GetFieldErrorList();
    IReadOnlyList<string> GetFieldErrorList(string name);
    bool TryGetFieldErrorList(string name, out IReadOnlyList<string> list);

    void ClearErrorList();
    void Reset();
    bool Validate();
    bool ValidateField(string name);
    bool TryValidateField(string name, out bool result);
    bool ValidateFieldList();
    void WriteError(string message);

    void HandleException(Exception exception);
    void HandleException(string message, Dictionary<string, IEnumerable<string>>? data = null);
}

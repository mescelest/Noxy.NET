namespace Noxy.NET.UI.Interfaces;

public interface IWebFormFieldContext
{
    string Name { get; }
    bool HasError { get; }
    bool HasChanged { get; }
    object? CurrentValue { get; }
    IReadOnlyList<string> ErrorList { get; }

    string? DisplayName { get; }
    string? Description { get; }
    void NotifyChange();

    void ClearErrorList();
    void Reset();
    bool Validate();
    void WriteError(string message);
    void WriteError(params string[] list);
}

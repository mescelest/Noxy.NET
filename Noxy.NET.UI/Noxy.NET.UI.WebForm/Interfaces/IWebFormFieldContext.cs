namespace Noxy.NET.UI.Interfaces;

public interface IWebFormFieldContext
{
    delegate void WebFormFieldContextEventHandler(IWebFormFieldContext sender);

    string Name { get; }
    bool HasError { get; }
    bool HasChanged { get; }

    event WebFormFieldContextEventHandler? Validated;
    event WebFormFieldContextEventHandler? Changed;

    string? GetDisplayName();
    string? GetDescription();
    string[] GetErrorList();
    void NotifyChange();

    void Clear();
    void Reset();
    bool Validate();
    void WriteError(string message);
    void WriteError(params string[] list);
}

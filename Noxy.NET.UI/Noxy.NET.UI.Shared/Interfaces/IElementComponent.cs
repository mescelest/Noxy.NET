namespace Noxy.NET.UI.Interfaces;

public interface IElementComponent : IBlazorComponent
{
    IReadOnlyDictionary<string, object>? AdditionalAttributes { get; init; }

    public string GetComponentClass();
    public bool TryExtractAttribute<T>(string attribute, out T? result);
}

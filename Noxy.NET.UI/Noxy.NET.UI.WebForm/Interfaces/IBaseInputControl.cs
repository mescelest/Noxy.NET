namespace Noxy.NET.UI.Interfaces;

public interface IBaseInputControl
{
    IReadOnlyDictionary<string, object>? InputAttributes { get; set; }
}

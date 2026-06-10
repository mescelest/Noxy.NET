using System.Diagnostics.CodeAnalysis;

namespace Noxy.NET.UI.Models;

public class ComponentMetadata
{
    public Type Type { get; }

    public string Name { get; }

    public Guid UUID { get; }
    public string UUIDString { get; }
    public string UUIDCode { get; }

    public bool IsRendered { get; private set; }

    public ComponentMetadata(Type type)
    {
        Type = type;
        Name = type.Name.Split('`').First();
        UUID = Guid.NewGuid();
        UUIDString = UUID.ToString();
        UUIDCode = UUIDString.Replace("-", "");
    }

    public void MarkAsRendered()
    {
        IsRendered = true;
    }

    public static string ExtractCssClass(IReadOnlyDictionary<string, object>? collection)
    {
        List<string> result = [];
        if (TryExtractAttribute(collection, "class", out string? @class) && !string.IsNullOrWhiteSpace(@class))
        {
            result.AddRange(@class.Split(' '));
        }

        return CombineCssClass([.. result]);
    }

    public static string CombineCssClass(params string?[] @params)
    {
        return string.Join(' ', @params.Where(x => !string.IsNullOrWhiteSpace(x)).OfType<string>().Select(@class => @class.Trim()));
    }

    public static bool TryExtractAttribute<T>(IReadOnlyDictionary<string, object>? collection, string attribute, [NotNullWhen(true)] out T? result)
    {
        if (collection is not null && collection.TryGetValue(attribute, out object? value) && value.GetType() == typeof(T))
        {
            result = (T)value;
            return true;
        }

        result = default;
        return false;
    }
}

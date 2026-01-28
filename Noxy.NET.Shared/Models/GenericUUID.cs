using System.Text.Json.Serialization;

namespace Noxy.NET.Models;

public class GenericUUID<T>(Guid? value)
{
    [JsonConstructor]
    public GenericUUID() : this(null)
    {
    }

    public Guid? Value { get; set; } = value;
    public string Name { get; set; } = typeof(T).Name;
}

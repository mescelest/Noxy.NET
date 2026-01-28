using System.Globalization;
using System.Reflection;

namespace Noxy.NET.Abstractions;

public abstract class Enumeration(int id, string name) : IComparable
{
    public int ID { get; set; } = id;
    public string Name { get; private set; } = name;

    public int CompareTo(object? obj) => obj is Enumeration value ? ID.CompareTo(value.ID) : 0;

    public override string ToString() => Name;
    public virtual string ToValueString() => ID.ToString(CultureInfo.InvariantCulture);
    public override bool Equals(object? obj) => obj is Enumeration otherValue && ID == otherValue.ID;
    public override int GetHashCode() => base.GetHashCode();

    public static IEnumerable<T> GetAll<T>() where T : Enumeration => typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(f => f.GetValue(null)).Cast<T>();
    public static T Get<T>(int id) where T : Enumeration => GetAll<T>().FirstOrDefault(x => x.ID == id) ?? throw new FormatException($"Cannot get Enumeration of type {typeof(T)} with {nameof(ID)}: '{id}'.");
    public static T Get<T>(string name) where T : Enumeration => GetAll<T>().FirstOrDefault(x => x.Name == name) ?? throw new FormatException($"Cannot get Enumeration of type {typeof(T)} with {nameof(Name)}: '{name}'.");
    public static bool TryGet<T>(int id, out T? value) where T : Enumeration => (value = GetAll<T>().FirstOrDefault(x => x.ID == id)) != null;
    public static bool TryGet<T>(string name, out T? value) where T : Enumeration => (value = GetAll<T>().FirstOrDefault(x => x.Name == name)) != null;

    public static bool operator ==(Enumeration? left, Enumeration? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(Enumeration? left, Enumeration? right) => !(left == right);
    public static bool operator <(Enumeration? left, Enumeration? right) => left is null ? right is not null : left.CompareTo(right) < 0;
    public static bool operator <=(Enumeration? left, Enumeration? right) => left is null || left.CompareTo(right) <= 0;
    public static bool operator >(Enumeration? left, Enumeration? right) => left is not null && left.CompareTo(right) > 0;
    public static bool operator >=(Enumeration? left, Enumeration? right) => left is null ? right is null : left.CompareTo(right) >= 0;
}

public abstract class Enumeration<T>(int id, string name, T value) : Enumeration(id, name)
{
    public T Value { get; private set; } = value;

    public override string ToValueString() => Value?.ToString() ?? string.Empty;
}

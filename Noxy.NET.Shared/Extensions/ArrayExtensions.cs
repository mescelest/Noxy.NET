namespace Noxy.NET.Extensions;

public static class ArrayExtensions
{
    public static T?[] AsNullableEnumArray<T>(this T[] value) where T : struct, Enum
    {
        return value.Select(x => (T?)(object)x).ToArray();
    }
}

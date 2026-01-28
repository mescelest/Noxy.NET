namespace Noxy.NET.Extensions;

public static class StringExtensions
{
    public static string DefaultIfEmpty(this string value, string defaultValue = "")
    {
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    public static string DefaultIfWhiteSpace(this string value, string defaultValue = "")
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}

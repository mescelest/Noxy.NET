using System.Reflection;
using Microsoft.JSInterop;

namespace Noxy.NET.UI.Extensions;

public static class JSRuntimeExtensions
{
    public static string GetComponentPath(this IJSRuntime self, Type componentType, bool local = false)
    {
        string? nameComponent = componentType.FullName;
        ArgumentException.ThrowIfNullOrWhiteSpace(nameComponent);

        string? nameAssembly = Assembly.GetCallingAssembly().GetName().Name;
        ArgumentException.ThrowIfNullOrWhiteSpace(nameAssembly);

        string nameParsed = ExtractUniqueSuffix(nameComponent, nameAssembly);
        string nameClassPartial = nameParsed.Split('`').First();
        string path = nameClassPartial.Replace('.', '/');

        return local ? $"./{path}.razor.js" : $"./_content/{nameAssembly}/{path}.razor.js";
    }

    private static string ExtractUniqueSuffix(string target, string reference, char separator = '.')
    {
        string[] refParts = reference.Split(separator);
        string[] tgtParts = target.Split(separator);
        int commonLength = Math.Min(refParts.Length, tgtParts.Length);
        int i = 0;

        while (i < commonLength && refParts[i] == tgtParts[i]) i++;

        return i < tgtParts.Length ? string.Join(separator, tgtParts[i..]) : string.Empty;
    }
}

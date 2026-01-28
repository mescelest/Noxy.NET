using System.IO.Hashing;
using System.Reflection;
using System.Text;
using Microsoft.JSInterop;

namespace Noxy.NET.UI.Extensions;

public static class JSRuntimeExtensions
{
    private static readonly HashSet<string> LoadedScripts = [];
    private static readonly HashSet<string> LoadedResources = [];

    public static async Task LoadScript(this IJSRuntime self, string code)
    {
        byte[] byteCode = Encoding.UTF8.GetBytes(code);
        byte[] byteHash = XxHash128.Hash(byteCode);
        string hexHash = Convert.ToHexString(byteHash);
        if (LoadedScripts.Add(hexHash)) await self.InvokeVoidAsync("eval", code);
    }

    public static async Task LoadScriptResource(this IJSRuntime self, string path, Assembly? assembly = null)
    {
        if (!LoadedResources.Add(path)) return;

        try
        {
            IJSObjectReference response = await self.InvokeAsync<IJSObjectReference>("fetch", path);
            string code = await response.InvokeAsync<string>("text");
            await LoadScript(self, code);
        }
        catch
        {
            LoadedResources.Remove(path);
            throw;
        }
    }

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

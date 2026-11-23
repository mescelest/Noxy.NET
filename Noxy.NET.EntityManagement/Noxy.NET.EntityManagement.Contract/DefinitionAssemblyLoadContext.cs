using System.Reflection;
using System.Runtime.Loader;
using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.EntityManagement.Contract;

// Loads a definition assembly with the files it needs from its own folder. The definitions library itself is the one this program uses, so an IDefinition of the assembly is the same type here.
public sealed class DefinitionAssemblyLoadContext(string assemblyPath) : AssemblyLoadContext
{
    private static readonly string SharedAssemblyName = typeof(IDefinition).Assembly.GetName().Name ?? throw new InvalidOperationException("The definitions assembly has no name.");

    private readonly AssemblyDependencyResolver _resolver = new(assemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == SharedAssemblyName) return null;

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }
}

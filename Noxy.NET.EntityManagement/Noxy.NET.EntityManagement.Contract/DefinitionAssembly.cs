using System.Reflection;
using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.EntityManagement.Contract;

public sealed class DefinitionAssembly(string path)
{
    // The definitions in the assembly: every public class that implements IDefinition and can be made without arguments, or only those with one of the names (the name of the type, or with its namespace).
    // Throws an ArgumentException for a file that is not there, a name that matches nothing, or an assembly with no definitions.
    public List<IDefinition> CreateDefinitionList(IReadOnlyList<string> nameList)
    {
        ArgumentNullException.ThrowIfNull(nameList);

        if (!File.Exists(path)) throw new ArgumentException($"There is no assembly at '{path}'.");

        Assembly assembly = new DefinitionAssemblyLoadContext(path).LoadFromAssemblyPath(path);

        List<Type> typeList = assembly.GetExportedTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false } && typeof(IDefinition).IsAssignableFrom(x) && x.GetConstructor(Type.EmptyTypes) is not null)
            .ToList();

        if (typeList.Count == 0) throw new ArgumentException($"'{path}' has no public class that implements IDefinition and can be made without arguments.");

        if (nameList.Count > 0)
        {
            string? unknown = nameList.FirstOrDefault(name => !typeList.Any(x => IsNamed(x, name)));
            if (unknown is not null) throw new ArgumentException($"There is no definition '{unknown}' in '{path}'. There is: {string.Join(", ", typeList.Select(x => x.FullName))}.");

            typeList = typeList.Where(x => nameList.Any(name => IsNamed(x, name))).ToList();
        }

        return typeList.Select(x => (IDefinition)(Activator.CreateInstance(x) ?? throw new InvalidOperationException($"{x.FullName} could not be made."))).ToList();
    }

    private static bool IsNamed(Type type, string name) => type.Name == name || type.FullName == name;
}

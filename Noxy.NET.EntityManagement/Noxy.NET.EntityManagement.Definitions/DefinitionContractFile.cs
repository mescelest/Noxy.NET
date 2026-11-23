using Noxy.NET.EntityManagement.Definitions.Extensions;

namespace Noxy.NET.EntityManagement.Definitions;

// The contract of a definition library, kept in a text file next to it (see DefinitionRegistryExtensions.GetContractLineList for what a line is).
// Definitions only change the contract on purpose: Verify fails on any difference, Update locks what is new and refuses a removal or a change, and only Update with acceptBreaks accepts one.
// The contract program (Noxy.NET.EntityManagement.Contract) runs these for a definition assembly.
public sealed class DefinitionContractFile(string path)
{
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));

    // The locked lines. A file that does not exist yet is an empty contract, so everything in the definitions is new.
    public List<string> ReadLineList()
    {
        if (!File.Exists(Path)) return [];

        return File.ReadAllLines(Path).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
    }

    public DefinitionContractResult Check(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        List<string> lockedList = ReadLineList();
        return new(registry.GetContractBreakList(lockedList), registry.GetContractAdditionList(lockedList));
    }

    // Writes the contract. The new lines are added to it; a removed or changed line stays locked and is refused with a DefinitionContractException, unless acceptBreaks is set: then the contract becomes the definitions as they are.
    // Returns what was added and what was dropped.
    public DefinitionContractResult Update(DefinitionRegistry registry, bool acceptBreaks = false)
    {
        DefinitionContractResult result = Check(registry);
        if (result.BreakList.Count > 0 && !acceptBreaks) throw new DefinitionContractException(Path, result);

        string? directory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path));
        if (directory is not null) Directory.CreateDirectory(directory);

        File.WriteAllLines(Path, registry.GetContractLineList());
        return result;
    }

    // For a test of the definition library: passes when the definitions match the contract, and throws a DefinitionContractException that says what differs when they do not. It never writes.
    public void Verify(DefinitionRegistry registry)
    {
        DefinitionContractResult result = Check(registry);
        if (!result.IsClean) throw new DefinitionContractException(Path, result);
    }
}

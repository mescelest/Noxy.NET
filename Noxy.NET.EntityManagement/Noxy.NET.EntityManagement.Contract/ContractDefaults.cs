using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.EntityManagement.Contract;

// What the questions start with. The last answer is kept in a small file of the user, and the first time the newest built definition assembly under the working folder is offered, with its contract file in the project folder.
public sealed class ContractDefaults(string settingsPath, string workingDirectory)
{
    public const string ContractFileName = "Contract.txt";
    private const string SettingsFolderName = "EntityManagement";
    private const string SettingsFileName = "contract-defaults.txt";
    private const string DefinitionAssemblyPattern = "*.Definitions.dll";
    private const string ProjectPattern = "*.csproj";

    public static ContractDefaults ForUser(string workingDirectory)
    {
        return new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SettingsFolderName, SettingsFileName), workingDirectory);
    }

    // The assembly of the last run if it is still there, else the newest built one found below the working folder, else null.
    public string? GetAssemblyPath()
    {
        string? remembered = ReadRemembered().AssemblyPath;
        if (remembered is not null && File.Exists(remembered)) return remembered;

        string ownName = $"{typeof(IDefinition).Assembly.GetName().Name}.dll";
        EnumerationOptions options = new() { RecurseSubdirectories = true, IgnoreInaccessible = true };

        return new DirectoryInfo(workingDirectory)
            .EnumerateFiles(DefinitionAssemblyPattern, options)
            .Where(x => x.Name != ownName && IsBuilt(x))
            .OrderByDescending(x => x.LastWriteTimeUtc)
            .Select(x => x.FullName)
            .FirstOrDefault();
    }

    // The contract file of the last run for this assembly, else Contract.txt in the folder of the project it was built from (or beside it).
    public string GetContractPath(string assemblyPath)
    {
        (string? rememberedAssembly, string? rememberedContract) = ReadRemembered();
        if (rememberedContract is not null && string.Equals(rememberedAssembly, assemblyPath, StringComparison.OrdinalIgnoreCase)) return rememberedContract;

        string folder = Path.GetDirectoryName(assemblyPath) ?? workingDirectory;
        for (DirectoryInfo? directory = new(folder); directory is not null; directory = directory.Parent)
        {
            if (directory.EnumerateFiles(ProjectPattern).Any()) return Path.Combine(directory.FullName, ContractFileName);
        }

        return Path.Combine(folder, ContractFileName);
    }

    public void Remember(ContractArguments arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string? directory = Path.GetDirectoryName(settingsPath);
        if (directory is not null) Directory.CreateDirectory(directory);

        File.WriteAllLines(settingsPath, [arguments.AssemblyPath, arguments.ContractPath]);
    }

    private static bool IsBuilt(FileInfo file)
    {
        // Not the copy in obj, which is not the finished build.
        return !file.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private (string? AssemblyPath, string? ContractPath) ReadRemembered()
    {
        if (!File.Exists(settingsPath)) return (null, null);

        string[] lineList = File.ReadAllLines(settingsPath);
        return (lineList.ElementAtOrDefault(0), lineList.ElementAtOrDefault(1));
    }
}

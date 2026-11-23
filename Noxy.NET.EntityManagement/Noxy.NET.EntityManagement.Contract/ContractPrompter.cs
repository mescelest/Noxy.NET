using Noxy.NET.EntityManagement.Contract.Enums;

namespace Noxy.NET.EntityManagement.Contract;

// Asks for the command, the assembly and the contract file, each with a default that an empty answer accepts.
public sealed class ContractPrompter(TextReader input, TextWriter output, ContractDefaults defaults)
{
    // Null when the input ends before everything is answered.
    public ContractArguments? Ask()
    {
        output.WriteLine("EntityManagement contract");
        output.WriteLine($"  {ContractArguments.CommandCheck}: compare and write nothing. {ContractArguments.CommandAdd}: lock what is new. {ContractArguments.CommandAccept}: accept removals and changes too.");
        output.WriteLine();

        ContractCommandEnum? command = null;
        string? commandText = Prompt($"Command ({ContractArguments.CommandCheck}, {ContractArguments.CommandAdd}, {ContractArguments.CommandAccept})", ContractArguments.CommandCheck, x => TryParseCommand(x, out command));
        if (commandText is null || command is null) return null;

        string? assembly = Prompt("Definition assembly (.dll)", defaults.GetAssemblyPath(), x => File.Exists(Path.GetFullPath(x)));
        if (assembly is null) return null;

        string assemblyPath = Path.GetFullPath(assembly);
        string? contract = Prompt("Contract file", defaults.GetContractPath(assemblyPath), x => !string.IsNullOrWhiteSpace(x));
        if (contract is null) return null;

        output.WriteLine();
        return new(command.Value, assemblyPath, Path.GetFullPath(contract), []);
    }

    private static bool TryParseCommand(string text, out ContractCommandEnum? command)
    {
        try
        {
            command = ContractArguments.ParseCommand(text);
            return true;
        }
        catch (ArgumentException)
        {
            command = null;
            return false;
        }
    }

    // Asks until the answer is valid. An empty answer is the default, and a path may be dragged in with its quotes. Null when the input ends.
    private string? Prompt(string label, string? defaultValue, Func<string, bool> isValid)
    {
        while (true)
        {
            output.Write(defaultValue is null ? $"{label}: " : $"{label} [{defaultValue}]: ");

            string? line = input.ReadLine();
            if (line is null) return null;

            string answer = line.Trim().Trim('"');
            if (answer.Length == 0 && defaultValue is not null) answer = defaultValue;

            if (answer.Length > 0 && isValid(answer)) return answer;

            output.WriteLine("That is not valid, try again.");
        }
    }
}

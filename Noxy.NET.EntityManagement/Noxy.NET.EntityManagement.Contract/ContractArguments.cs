using Noxy.NET.EntityManagement.Contract.Enums;

namespace Noxy.NET.EntityManagement.Contract;

public sealed record ContractArguments(ContractCommandEnum Command, string AssemblyPath, string ContractPath, IReadOnlyList<string> DefinitionNameList)
{
    public const string DefinitionOption = "--definition";
    public const string CommandCheck = "check";
    public const string CommandAdd = "add";
    public const string CommandAccept = "accept";

    public const string Usage = $"""
                                 Usage: em-contract <{CommandCheck}|{CommandAdd}|{CommandAccept}> <definition assembly> <contract file> [{DefinitionOption} <type>]...
                                        em-contract            (no arguments: asks for each value, with a default)

                                   {CommandCheck}   compare the definitions with the contract file and write nothing
                                   {CommandAdd}     lock the lines that are new in the contract file; refuses when something locked was removed or changed
                                   {CommandAccept}  make the contract file the definitions as they are now, also when something locked was removed or changed

                                 The assembly is the built .dll of the definition library. Every public IDefinition in it is used, or only the ones named with {DefinitionOption}.
                                 """;

    // Throws an ArgumentException whose message says what is wrong, for a person to read.
    public static ContractCommandEnum ParseCommand(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.Trim().ToLowerInvariant() switch
        {
            CommandCheck => ContractCommandEnum.Check,
            CommandAdd => ContractCommandEnum.Add,
            CommandAccept => ContractCommandEnum.Accept,
            _ => throw new ArgumentException($"Unknown command '{text}'.")
        };
    }

    // Throws an ArgumentException whose message says what is wrong, for a person to read.
    public static ContractArguments Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        List<string> positionList = [];
        List<string> definitionNameList = [];

        for (int i = 0; i < args.Count; i++)
        {
            if (args[i] == DefinitionOption)
            {
                if (i + 1 >= args.Count) throw new ArgumentException($"{DefinitionOption} needs the name of a type.");
                definitionNameList.Add(args[++i]);
            }
            else if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unknown option '{args[i]}'.");
            }
            else
            {
                positionList.Add(args[i]);
            }
        }

        if (positionList.Count != 3) throw new ArgumentException("Expected a command, a definition assembly and a contract file.");

        return new(ParseCommand(positionList[0]), Path.GetFullPath(positionList[1]), Path.GetFullPath(positionList[2]), definitionNameList);
    }
}

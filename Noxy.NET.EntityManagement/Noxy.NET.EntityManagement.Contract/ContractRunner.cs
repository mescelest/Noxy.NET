using System.ComponentModel;
using Noxy.NET.EntityManagement.Contract.Enums;
using Noxy.NET.EntityManagement.Definitions;

namespace Noxy.NET.EntityManagement.Contract;

// Runs one command of the program. The result is the exit code: 0 when it is done or the definitions match, 1 when they differ and nothing was written, 2 when it could not run.
public sealed class ContractRunner(TextWriter output, TextWriter error)
{
    public const int ExitDone = 0;
    public const int ExitDiffers = 1;
    public const int ExitFailed = 2;

    public int Run(IReadOnlyList<string> args)
    {
        ContractArguments arguments;

        try
        {
            arguments = ContractArguments.Parse(args);
        }
        catch (ArgumentException exception)
        {
            error.WriteLine(exception.Message);
            error.WriteLine();
            error.WriteLine(ContractArguments.Usage);
            return ExitFailed;
        }

        return Run(arguments);
    }

    public int Run(ContractArguments arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        DefinitionRegistry registry;

        try
        {
            registry = DefinitionBuilder.Create([.. new DefinitionAssembly(arguments.AssemblyPath).CreateDefinitionList(arguments.DefinitionNameList)]);
        }
        catch (ArgumentException exception)
        {
            error.WriteLine(exception.Message);
            return ExitFailed;
        }
        catch (DefinitionException exception)
        {
            error.WriteLine("The definitions are not valid, so there is nothing to compare:");
            error.WriteLine(exception.Message);
            return ExitFailed;
        }

        DefinitionContractFile file = new(arguments.ContractPath);

        return arguments.Command switch
        {
            ContractCommandEnum.Check => Check(file, registry),
            ContractCommandEnum.Add => Update(file, registry, false),
            ContractCommandEnum.Accept => Update(file, registry, true),
            _ => throw new InvalidEnumArgumentException(nameof(arguments.Command), (int)arguments.Command, typeof(ContractCommandEnum))
        };
    }

    private int Check(DefinitionContractFile file, DefinitionRegistry registry)
    {
        DefinitionContractResult result = file.Check(registry);

        if (result.IsClean)
        {
            output.WriteLine($"The definitions match the contract in '{file.Path}'.");
            return ExitDone;
        }

        error.WriteLine(new DefinitionContractException(file.Path, result).Message);
        return ExitDiffers;
    }

    private int Update(DefinitionContractFile file, DefinitionRegistry registry, bool acceptBreaks)
    {
        try
        {
            DefinitionContractResult result = file.Update(registry, acceptBreaks);

            Write("Locked", result.AdditionList);
            Write("Dropped from the contract", result.BreakList);
            output.WriteLine(result.IsClean ? $"Nothing to change: the definitions already match the contract in '{file.Path}'." : $"The contract in '{file.Path}' is up to date.");

            return ExitDone;
        }
        catch (DefinitionContractException exception)
        {
            error.WriteLine(exception.Message);
            return ExitDiffers;
        }
    }

    private void Write(string title, IReadOnlyList<string> lineList)
    {
        if (lineList.Count == 0) return;

        output.WriteLine($"{title}:");
        foreach (string line in lineList) output.WriteLine($"  {line}");
    }
}

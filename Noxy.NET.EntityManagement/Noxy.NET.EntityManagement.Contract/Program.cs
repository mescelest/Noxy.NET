using Noxy.NET.EntityManagement.Contract;

ContractRunner runner = new(Console.Out, Console.Error);

if (args.Length > 0) return runner.Run(args);

// No arguments: ask for them, with defaults, and keep the window open so the result can be read.
ContractDefaults defaults = ContractDefaults.ForUser(Directory.GetCurrentDirectory());
ContractArguments? chosen = new ContractPrompter(Console.In, Console.Out, defaults).Ask();
if (chosen is null) return ContractRunner.ExitFailed;

defaults.Remember(chosen);
int exitCode = runner.Run(chosen);

Console.Out.WriteLine();
Console.Out.WriteLine("Press Enter to close.");
Console.In.ReadLine();

return exitCode;

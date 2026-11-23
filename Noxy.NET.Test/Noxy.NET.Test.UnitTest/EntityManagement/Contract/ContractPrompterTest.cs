using Noxy.NET.EntityManagement.Contract;
using Noxy.NET.EntityManagement.Contract.Enums;

namespace Noxy.NET.Test.EntityManagement.Contract;

public class ContractPrompterTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "em-contract-ask-" + Guid.NewGuid().ToString("N"));
    private readonly StringWriter _output = new();

    public ContractPrompterTest()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private string SettingsPath => Path.Combine(_directory, "settings", "defaults.txt");

    // A project folder with a built definition assembly, and a copy in obj that must not be offered.
    private string MakeProject(string name, DateTime built)
    {
        string project = Path.Combine(_directory, name);
        string bin = Path.Combine(project, "bin", "Debug", "net10.0");
        Directory.CreateDirectory(bin);
        Directory.CreateDirectory(Path.Combine(project, "obj"));
        File.WriteAllText(Path.Combine(project, $"{name}.csproj"), "<Project/>");

        string assembly = Path.Combine(bin, $"{name}.Definitions.dll");
        File.WriteAllText(assembly, "");
        File.SetLastWriteTimeUtc(assembly, built);
        File.WriteAllText(Path.Combine(project, "obj", $"{name}.Definitions.dll"), "");

        return assembly;
    }

    private ContractPrompter Prompter(string answers, ContractDefaults defaults) => new(new StringReader(answers), _output, defaults);

    [Fact]
    public void TheFirstTime_TheNewestBuiltAssemblyIsTheDefault_WithItsContractInTheProjectFolder()
    {
        MakeProject("Old", new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        string newest = MakeProject("New", new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        ContractDefaults defaults = new(SettingsPath, _directory);

        ContractArguments? arguments = Prompter("\n\n\n", defaults).Ask();

        Assert.NotNull(arguments);
        Assert.Equal(ContractCommandEnum.Check, arguments.Command);
        Assert.Equal(newest, arguments.AssemblyPath);
        Assert.Equal(Path.Combine(_directory, "New", ContractDefaults.ContractFileName), arguments.ContractPath);
    }

    [Fact]
    public void WhatIsTyped_ReplacesTheDefaults_AndQuotesAroundAPathAreTakenOff()
    {
        string assembly = MakeProject("New", DateTime.UtcNow);
        string contract = Path.Combine(_directory, "somewhere", "Own.txt");

        ContractArguments? arguments = Prompter($"add\n\"{assembly}\"\n\"{contract}\"\n", new(SettingsPath, _directory)).Ask();

        Assert.NotNull(arguments);
        Assert.Equal((ContractCommandEnum.Add, assembly, contract), (arguments.Command, arguments.AssemblyPath, arguments.ContractPath));
    }

    [Fact]
    public void AnInvalidAnswer_IsAskedAgain()
    {
        string assembly = MakeProject("New", DateTime.UtcNow);

        ContractArguments? arguments = Prompter($"dance\naccept\n{Path.Combine(_directory, "missing.dll")}\n{assembly}\n\n", new(SettingsPath, _directory)).Ask();

        Assert.NotNull(arguments);
        Assert.Equal(ContractCommandEnum.Accept, arguments.Command);
        Assert.Equal(assembly, arguments.AssemblyPath);
        Assert.Equal(2, _output.ToString().Split("That is not valid, try again.").Length - 1);
    }

    [Fact]
    public void WhenThereIsNoDefaultAssembly_ItHasToBeTyped_AndWhenTheInputEnds_NothingIsReturned()
    {
        Assert.Null(Prompter("\n\n", new(SettingsPath, _directory)).Ask());
        Assert.Contains("That is not valid", _output.ToString());
    }

    [Fact]
    public void TheLastAnswers_AreTheDefaultsNextTime()
    {
        string first = MakeProject("First", new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        MakeProject("Second", new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        string contract = Path.Combine(_directory, "Chosen.txt");
        ContractDefaults defaults = new(SettingsPath, _directory);

        defaults.Remember(new(ContractCommandEnum.Add, first, contract, []));
        ContractArguments? arguments = Prompter("\n\n\n", defaults).Ask();

        Assert.NotNull(arguments);
        Assert.Equal((first, contract), (arguments.AssemblyPath, arguments.ContractPath));
    }
}

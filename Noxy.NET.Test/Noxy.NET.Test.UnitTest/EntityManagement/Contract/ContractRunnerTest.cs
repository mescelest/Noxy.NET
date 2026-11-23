using Noxy.NET.EntityManagement.Contract;
using Noxy.NET.EntityManagement.Contract.Enums;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Extensions;
using Noxy.NET.EntityManagement.Demo.Definitions;

namespace Noxy.NET.Test.EntityManagement.Contract;

public class ContractRunnerTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "em-contract-run-" + Guid.NewGuid().ToString("N"));
    private readonly StringWriter _output = new();
    private readonly StringWriter _error = new();

    // The built demo definitions, loaded the way the program loads any definition assembly.
    private static readonly string DemoAssemblyPath = typeof(CaseworkDefinition).Assembly.Location;

    public ContractRunnerTest()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private string ContractPath => Path.Combine(_directory, "Contract.txt");

    private int Run(params string[] args) => new ContractRunner(_output, _error).Run(args);

    private static List<string> DemoLineList() => DefinitionBuilder.Create(new CaseworkDefinition()).GetContractLineList();

    [Fact]
    public void Check_WithNoContractYet_Differs_AndWritesNothing()
    {
        int exitCode = Run("check", DemoAssemblyPath, ContractPath);

        Assert.Equal(ContractRunner.ExitDiffers, exitCode);
        Assert.False(File.Exists(ContractPath));
        Assert.Contains("New, and not locked yet", _error.ToString());
    }

    [Fact]
    public void Add_LocksTheDefinitionsOfTheAssembly_AndCheckThenMatches()
    {
        Assert.Equal(ContractRunner.ExitDone, Run("add", DemoAssemblyPath, ContractPath));
        Assert.Equal(DemoLineList(), File.ReadAllLines(ContractPath));

        Assert.Equal(ContractRunner.ExitDone, Run("check", DemoAssemblyPath, ContractPath));
        Assert.Contains("match the contract", _output.ToString());
    }

    [Fact]
    public void Add_RefusesAndLeavesTheFileAlone_WhenSomethingLockedIsGone_AndAcceptDropsIt()
    {
        File.WriteAllLines(ContractPath, DemoLineList().Append("property ghost String").Order(StringComparer.Ordinal));
        string before = File.ReadAllText(ContractPath);

        Assert.Equal(ContractRunner.ExitDiffers, Run("add", DemoAssemblyPath, ContractPath));
        Assert.Equal(before, File.ReadAllText(ContractPath));
        Assert.Contains("property ghost String", _error.ToString());

        Assert.Equal(ContractRunner.ExitDone, Run("accept", DemoAssemblyPath, ContractPath));
        Assert.Equal(DemoLineList(), File.ReadAllLines(ContractPath));
    }

    [Fact]
    public void ADefinitionCanBeNamed_AndANameThatIsNotThere_FailsWithTheOnesThatAre()
    {
        Assert.Equal(ContractRunner.ExitDone, Run("add", DemoAssemblyPath, ContractPath, "--definition", nameof(CaseworkDefinition)));

        Assert.Equal(ContractRunner.ExitFailed, Run("check", DemoAssemblyPath, ContractPath, "--definition", "Nope"));
        Assert.Contains(typeof(CaseworkDefinition).FullName!, _error.ToString());
    }

    [Fact]
    public void BadArguments_FailWithTheUsage()
    {
        Assert.Equal(ContractRunner.ExitFailed, Run("check"));
        Assert.Equal(ContractRunner.ExitFailed, Run("dance", DemoAssemblyPath, ContractPath));
        Assert.Equal(ContractRunner.ExitFailed, Run("check", DemoAssemblyPath, ContractPath, "--what"));
        Assert.Equal(ContractRunner.ExitFailed, Run("check", Path.Combine(_directory, "missing.dll"), ContractPath));

        Assert.Contains("Usage:", _error.ToString());
    }

    [Fact]
    public void TheArguments_AreParsedWithTheCommandInAnyCase()
    {
        ContractArguments arguments = ContractArguments.Parse(["ADD", "a.dll", "c.txt", "--definition", "One", "--definition", "Two"]);

        Assert.Equal(ContractCommandEnum.Add, arguments.Command);
        Assert.Equal(Path.GetFullPath("a.dll"), arguments.AssemblyPath);
        Assert.Equal(Path.GetFullPath("c.txt"), arguments.ContractPath);
        Assert.Equal(["One", "Two"], arguments.DefinitionNameList);
    }
}

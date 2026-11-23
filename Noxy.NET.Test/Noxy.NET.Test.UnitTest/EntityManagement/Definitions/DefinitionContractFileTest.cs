using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Extensions;
using Noxy.NET.EntityManagement.Definitions.Interfaces;

namespace Noxy.NET.Test.EntityManagement.Definitions;

public class DefinitionContractFileTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "em-contract-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private DefinitionContractFile NewFile() => new(Path.Combine(_directory, "Contract.txt"));

    private static DefinitionRegistry Registry(Action<IDefinitionBuilder> define) => DefinitionBuilder.Create(new ActionDefinition(define));

    private static void Basic(IDefinitionBuilder builder, PropertyKindEnum kind = PropertyKindEnum.String)
    {
        builder.AddTextParameter("title", TextKindEnum.Line);
        builder.AddProperty("name", kind, "title");
        builder.AddElement("person", "title", ["name"]);
    }

    private static DefinitionRegistry Grown() => Registry(x =>
    {
        Basic(x);
        x.AddProperty("age", PropertyKindEnum.Integer, "title");
        x.AddElement("case", "title", ["age"]);
    });

    [Fact]
    public void AFileThatIsNotThereYet_IsAnEmptyContract_SoEverythingIsNew()
    {
        DefinitionContractFile file = NewFile();

        Assert.Empty(file.ReadLineList());
        Assert.Equal(Registry(x => Basic(x)).GetContractLineList(), file.Check(Registry(x => Basic(x))).AdditionList);
        Assert.Empty(file.Check(Registry(x => Basic(x))).BreakList);
    }

    [Fact]
    public void Updating_WritesTheNewLines_AndTheDefinitionsThenMatch()
    {
        DefinitionContractFile file = NewFile();
        DefinitionRegistry registry = Registry(x => Basic(x));

        DefinitionContractResult result = file.Update(registry);

        Assert.NotEmpty(result.AdditionList);
        Assert.True(file.Check(registry).IsClean);
        file.Verify(registry);
    }

    [Fact]
    public void ANewDefinition_FailsTheCheck_UntilItIsLockedOnPurpose_AndTheOldLinesStay()
    {
        DefinitionContractFile file = NewFile();
        file.Update(Registry(x => Basic(x)));

        DefinitionContractException exception = Assert.Throws<DefinitionContractException>(() => file.Verify(Grown()));
        Assert.Empty(exception.Result.BreakList);
        Assert.Contains("element case", exception.Result.AdditionList);
        Assert.Contains("'add'", exception.Message);

        DefinitionContractResult result = file.Update(Grown());

        Assert.Empty(result.BreakList);
        Assert.Contains("element person", file.ReadLineList());
        Assert.Contains("element case", file.ReadLineList());
        file.Verify(Grown());
    }

    [Fact]
    public void ARetypedProperty_IsRefusedByUpdate_AndTheFileIsLeftAsItWas()
    {
        DefinitionContractFile file = NewFile();
        file.Update(Registry(x => Basic(x)));
        List<string> before = file.ReadLineList();

        DefinitionContractException exception = Assert.Throws<DefinitionContractException>(() => file.Update(Registry(x => Basic(x, PropertyKindEnum.Integer))));

        Assert.Equal(["property name String"], exception.Result.BreakList);
        Assert.Equal(before, file.ReadLineList());
        Assert.Contains("'accept'", exception.Message);
    }

    [Fact]
    public void AcceptingABreak_MakesTheContractTheDefinitionsAsTheyAre()
    {
        DefinitionContractFile file = NewFile();
        file.Update(Registry(x => Basic(x)));
        DefinitionRegistry retyped = Registry(x => Basic(x, PropertyKindEnum.Integer));

        DefinitionContractResult result = file.Update(retyped, acceptBreaks: true);

        Assert.Equal(["property name String"], result.BreakList);
        Assert.Contains("property name Integer", file.ReadLineList());
        Assert.DoesNotContain("property name String", file.ReadLineList());
        file.Verify(retyped);
    }

    [Fact]
    public void Verifying_NeverWrites()
    {
        DefinitionContractFile file = NewFile();

        Assert.Throws<DefinitionContractException>(() => file.Verify(Registry(x => Basic(x))));

        Assert.False(File.Exists(file.Path));
    }
}

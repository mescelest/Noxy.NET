using System.Runtime.CompilerServices;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Definitions.Extensions;
using Noxy.NET.EntityManagement.Definitions.Interfaces;
using Noxy.NET.EntityManagement.Demo.Definitions;

namespace Noxy.NET.Test.EntityManagement.Definitions;

// The demo definitions may grow, but what is in CaseworkContract.txt may not be removed, renamed or changed in kind: data in a database is stored under those identifiers.
// When this fails because something is new, lock it on purpose with the contract program (Noxy.NET.EntityManagement.Contract, command 'add').
public class CaseworkContractTest
{
    private static DefinitionRegistry Build() => DefinitionBuilder.Create(new CaseworkDefinition());

    // The contract file of the demo definitions, in the source of that project.
    private static DefinitionContractFile ContractFile([CallerFilePath] string testFilePath = "")
    {
        string folder = Path.GetDirectoryName(testFilePath) ?? throw new InvalidOperationException("The path of the test file is not known.");
        return new(Path.GetFullPath(Path.Combine(folder, "..", "..", "..", "..", "Noxy.NET.EntityManagement.Demo", "Noxy.NET.EntityManagement.Demo.Definitions", "Contract.txt")));
    }

    [Fact]
    public void TheDefinitions_MatchTheContract()
    {
        ContractFile().Verify(Build());
    }

    [Fact]
    public void TheLockedFile_IsSortedAndHasNoDuplicates()
    {
        List<string> locked = ContractFile().ReadLineList();

        Assert.Equal(locked.Order(StringComparer.Ordinal).Distinct().ToList(), locked);
    }

    [Fact]
    public void TheLines_AreTheSameEachTimeTheyAreBuilt()
    {
        Assert.Equal(Build().GetContractLineList(), Build().GetContractLineList());
    }

    #region -- What counts as a break --

    private static DefinitionRegistry Registry(Action<IDefinitionBuilder> define) => DefinitionBuilder.Create(new ActionDefinition(define));

    private static void Define(IDefinitionBuilder builder, PropertyKindEnum kind = PropertyKindEnum.String, string property = "name", string element = "person", RelationCardinalityEnum cardinality = RelationCardinalityEnum.ManyToMany)
    {
        builder.AddTextParameter("title", TextKindEnum.Line);
        builder.AddProperty(property, kind, "title");
        builder.AddElement(element, "title", [property]);
        builder.AddElement("case", "title", [property]);
        builder.AddRelation("involved", element, "case", "title", "title", cardinality);
        builder.AddContext("file", "title", [element, "case"]);
    }

    private static List<string> Locked() => Registry(x => Define(x)).GetContractLineList();

    [Fact]
    public void ANewIdentifier_IsAnAddition_NotABreak()
    {
        DefinitionRegistry grown = Registry(x =>
        {
            Define(x);
            x.AddProperty("age", PropertyKindEnum.Integer, "title");
            x.AddElement("vehicle", "title", ["age"]);
        });

        Assert.Empty(grown.GetContractBreakList(Locked()));
        Assert.Equal(["element vehicle", "element vehicle property age", "property age Integer"], grown.GetContractAdditionList(Locked()).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ARenamedProperty_IsABreak()
    {
        DefinitionRegistry renamed = Registry(x => Define(x, property: "title-of-person"));

        Assert.Contains("property name String", renamed.GetContractBreakList(Locked()));
        Assert.Contains("element person property name", renamed.GetContractBreakList(Locked()));
    }

    [Fact]
    public void ARetypedProperty_IsABreak()
    {
        DefinitionRegistry retyped = Registry(x => Define(x, kind: PropertyKindEnum.Integer));

        Assert.Equal(["property name String"], retyped.GetContractBreakList(Locked()));
    }

    [Fact]
    public void ARenamedElement_IsABreak()
    {
        DefinitionRegistry renamed = Registry(x => Define(x, element: "human"));

        Assert.Contains("element person", renamed.GetContractBreakList(Locked()));
    }

    [Fact]
    public void ARelationThatChangesCardinality_IsABreak()
    {
        DefinitionRegistry changed = Registry(x => Define(x, cardinality: RelationCardinalityEnum.OneToOne));

        Assert.Equal(["relation involved person case ManyToMany"], changed.GetContractBreakList(Locked()));
    }

    [Fact]
    public void ARemovedContext_IsABreak()
    {
        DefinitionRegistry removed = Registry(x =>
        {
            x.AddTextParameter("title", TextKindEnum.Line);
            x.AddProperty("name", PropertyKindEnum.String, "title");
            x.AddElement("person", "title", ["name"]);
            x.AddElement("case", "title", ["name"]);
            x.AddRelation("involved", "person", "case", "title", "title");
        });

        Assert.Equal(["context file"], removed.GetContractBreakList(Locked()));
    }

    [Fact]
    public void AChangeOfTitleOrOrderOrRequired_IsNotInTheContract()
    {
        DefinitionRegistry reordered = Registry(x =>
        {
            x.AddTextParameter("title", TextKindEnum.Line);
            x.AddTextParameter("other", TextKindEnum.Line);
            x.AddProperty("name", PropertyKindEnum.String, "other", isRequired: true, maxLength: 5);
            x.AddElement("person", "other", ["name"]);
            x.AddElement("case", "title", ["name"]);
            x.AddRelation("involved", "person", "case", "title", "title");
            x.AddContext("file", "title", ["case", "person"]);
        });

        Assert.Empty(reordered.GetContractBreakList(Locked()));
        Assert.Equal(["text other Line"], reordered.GetContractAdditionList(Locked()));
    }

    #endregion
}

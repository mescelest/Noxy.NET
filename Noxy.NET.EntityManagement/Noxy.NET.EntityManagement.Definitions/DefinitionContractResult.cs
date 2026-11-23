namespace Noxy.NET.EntityManagement.Definitions;

// How the definitions stand against the contract file. BreakList: locked lines that are gone or changed. AdditionList: lines in the definitions that are not locked yet.
public sealed record DefinitionContractResult(IReadOnlyList<string> BreakList, IReadOnlyList<string> AdditionList)
{
    public bool IsClean => BreakList.Count == 0 && AdditionList.Count == 0;
}

namespace Noxy.NET.EntityManagement.Definitions;

// Thrown when the definitions are built and something is wrong. Every problem found is in ErrorList, not only the first.
public sealed class DefinitionException(IReadOnlyList<string> errorList) : Exception(CreateMessage(errorList))
{
    public IReadOnlyList<string> ErrorList { get; } = errorList;

    private static string CreateMessage(IReadOnlyList<string> errorList)
    {
        return $"The definitions are not valid ({errorList.Count} problem{(errorList.Count == 1 ? string.Empty : "s")}):{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errorList)}";
    }
}

using Noxy.NET.EntityManagement.Application.Models;

namespace Noxy.NET.EntityManagement.Application;

// Thrown when the values of an element are not valid. Every problem found is in ErrorList, not only the first, so a form can show them all.
public sealed class DataValidationException(IReadOnlyList<DataError> errorList) : Exception(CreateMessage(errorList))
{
    public IReadOnlyList<DataError> ErrorList { get; } = errorList;

    private static string CreateMessage(IReadOnlyList<DataError> errorList)
    {
        return $"The values are not valid ({errorList.Count} problem{(errorList.Count == 1 ? string.Empty : "s")}):{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errorList.Select(x => x.Message))}";
    }
}

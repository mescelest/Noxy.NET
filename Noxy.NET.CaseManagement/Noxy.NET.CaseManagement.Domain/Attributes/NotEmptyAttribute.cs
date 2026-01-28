using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.CaseManagement.Domain.Attributes;

public class NotEmptyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Guid guid && guid == Guid.Empty)
        {
            return new($"The field {validationContext.DisplayName} cannot be an empty GUID.");
        }

        return ValidationResult.Success;
    }
}

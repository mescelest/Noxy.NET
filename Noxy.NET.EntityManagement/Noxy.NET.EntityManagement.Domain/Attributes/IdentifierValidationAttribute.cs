using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Noxy.NET.EntityManagement.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed partial class IdentifierValidationAttribute : ValidationAttribute
{
    [GeneratedRegex(@"^[\p{L}_][\p{L}\p{Nl}\p{Nd}\p{Pc}\p{Mn}\p{Mc}\p{Cf}]*$", RegexOptions.Compiled)]
    private static partial Regex IdentifierRegex();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        IEnumerable<string>? listMemberName = !string.IsNullOrEmpty(validationContext.MemberName) ? [validationContext.MemberName] : null;

        if (value is not string parsed) return new($"{validationContext.MemberName} must be a string.", listMemberName);

        return IdentifierRegex().IsMatch(parsed)
            ? ValidationResult.Success
            : new($"{validationContext.MemberName} must follow C# identifier naming rules.", listMemberName);
    }
}

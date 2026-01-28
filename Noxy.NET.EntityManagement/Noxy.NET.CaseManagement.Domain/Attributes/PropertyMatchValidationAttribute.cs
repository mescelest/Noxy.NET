using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Noxy.NET.CaseManagement.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PropertyMatchValidationAttribute(string otherPropertyName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        IEnumerable<string>? listMemberName = !string.IsNullOrEmpty(validationContext.MemberName) ? [validationContext.MemberName] : null;

        PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(otherPropertyName);
        if (otherProperty == null)
        {
            return new($"Unknown property: {otherPropertyName}", listMemberName);
        }

        object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
        if (!EqualityComparer<object>.Default.Equals(value, otherValue))
        {
            return new($"{validationContext.MemberName} must match {otherPropertyName}.", listMemberName);
        }

        return ValidationResult.Success;
    }
}

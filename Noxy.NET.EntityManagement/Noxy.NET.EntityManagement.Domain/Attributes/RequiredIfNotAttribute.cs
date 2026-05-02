using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Noxy.NET.EntityManagement.Domain.Attributes;

public class RequiredIfNotAttribute(params string[] otherPropertyNames) : ValidationAttribute
{
    private string[] OtherPropertyNames { get; } = otherPropertyNames;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        List<object?> values = [value];

        foreach (string propName in OtherPropertyNames)
        {
            PropertyInfo? property = context.ObjectType.GetProperty(propName);
            if (property == null) return new($"Unknown property: {propName}");

            object? otherValue = property.GetValue(context.ObjectInstance);
            values.Add(otherValue);
        }

        if (values.Any(HasValue)) return ValidationResult.Success;

        string[] allProps = new string[OtherPropertyNames.Length + 1];
        allProps[0] = context.MemberName ?? "ThisProperty";
        OtherPropertyNames.CopyTo(allProps, 1);

        return new(ErrorMessage ?? $"At least one of these properties must have a value: {string.Join(", ", allProps)}");
    }

    private static bool HasValue(object? value)
    {
        return value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            Guid g => g != Guid.Empty,
            _ => true
        };
    }
}

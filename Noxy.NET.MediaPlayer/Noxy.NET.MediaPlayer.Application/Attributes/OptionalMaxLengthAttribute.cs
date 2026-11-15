using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models;

namespace Noxy.NET.MediaPlayer.Application.Attributes;

// A field that was left out, or sent as null, is fine. Text that was sent can be at most MaxLength characters.
[AttributeUsage(AttributeTargets.Property)]
public class OptionalMaxLengthAttribute(int maxLength) : ValidationAttribute("The field {0} can be at most {1} characters.")
{
    public int MaxLength { get; } = maxLength;

    public override bool IsValid(object? value)
    {
        if (value is not IOptional { HasValue: true } optional) return true;

        return optional.UntypedValue is not string text || text.Length <= MaxLength;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MaxLength);
    }
}

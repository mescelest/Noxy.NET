using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models;

namespace Noxy.NET.MediaPlayer.Application.Attributes;

// A field that was left out, or sent as null, is fine. A number that was sent must be within the range.
[AttributeUsage(AttributeTargets.Property)]
public class OptionalRangeAttribute(int minimum, int maximum) : ValidationAttribute("The field {0} must be between {1} and {2}.")
{
    public int Minimum { get; } = minimum;
    public int Maximum { get; } = maximum;

    public override bool IsValid(object? value)
    {
        if (value is not IOptional { HasValue: true } optional) return true;

        return optional.UntypedValue is not int number || (number >= Minimum && number <= Maximum);
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum);
    }
}

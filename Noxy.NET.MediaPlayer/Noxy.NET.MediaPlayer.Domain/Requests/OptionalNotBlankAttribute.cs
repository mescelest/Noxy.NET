using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Domain.Requests;

// A field that was left out is fine. One that was sent must be text with something in it.
[AttributeUsage(AttributeTargets.Property)]
public class OptionalNotBlankAttribute() : ValidationAttribute("The field {0} can't be empty.")
{
    public override bool IsValid(object? value)
    {
        if (value is not IOptional { HasValue: true } optional) return true;

        return optional.UntypedValue is string text && !string.IsNullOrWhiteSpace(text);
    }
}

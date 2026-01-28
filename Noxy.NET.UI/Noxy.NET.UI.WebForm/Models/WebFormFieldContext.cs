using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class WebFormFieldContext : IWebFormFieldContext
{
    internal WebFormFieldContext(string name, object model)
    {
        Name = name;
        ValidationContext = new(model) { MemberName = name };
    }

    private ValidationContext ValidationContext { get; }
    private List<ValidationResult> ValidationResultList { get; } = [];
    public string Name { get; }
    public bool HasChanged { get; private set; }
    public bool HasError { get; private set; }

    public event IWebFormFieldContext.WebFormFieldContextEventHandler? Validated;
    public event IWebFormFieldContext.WebFormFieldContextEventHandler? Changed;

    public string? GetDisplayName()
    {
        PropertyInfo? property = GetPropertyInfo();
        DisplayNameAttribute? attr = property?.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
        return attr?.DisplayName;
    }

    public string? GetDescription()
    {
        PropertyInfo? property = GetPropertyInfo();
        DescriptionAttribute? attr = property?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
        return attr?.Description;
    }

    public string[] GetErrorList()
    {
        return ValidationResultList.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray();
    }

    public void NotifyChange()
    {
        HasChanged = true;
        Validate();
    }

    public void Clear()
    {
        HasError = false;
        ValidationResultList.Clear();
    }

    public void Reset()
    {
        HasChanged = false;
        Clear();
    }

    public bool Validate()
    {
        Clear();

        PropertyInfo? property = GetPropertyInfo();
        object? value = property?.GetValue(ValidationContext.ObjectInstance);

        HasError = Validator.TryValidateProperty(value, ValidationContext, ValidationResultList);
        Validated?.Invoke(this);

        return HasError;
    }

    public void WriteError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ValidationResultList.Add(new(message, [Name]));
    }

    public void WriteError(params string[] list)
    {
        foreach (string message in list)
        {
            WriteError(message);
        }
    }

    private PropertyInfo? GetPropertyInfo()
    {
        return ValidationContext.ObjectInstance.GetType().GetProperty(Name);
    }
}

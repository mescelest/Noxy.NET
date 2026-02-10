using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class WebFormFieldContext : IWebFormFieldContext
{
    private readonly DescriptionAttribute? _description;
    private readonly DisplayNameAttribute? _displayName;
    private readonly List<string> _listError = [];

    private readonly object _model;
    private readonly object? _originalValue;
    private readonly PropertyInfo _property;


    internal WebFormFieldContext(string name, object model)
    {
        Name = name;
        _model = model;

        _property = model.GetType().GetProperty(name) ?? throw new InvalidOperationException($"Property '{name}' does not exist on model type '{model.GetType().Name}'.");
        _originalValue = _property.GetValue(model);

        _displayName = _property.GetCustomAttribute<DisplayNameAttribute>();
        _description = _property.GetCustomAttribute<DescriptionAttribute>();
    }

    public string Name { get; }
    public string? DisplayName => _displayName?.DisplayName;
    public string? Description => _description?.Description;
    public bool HasChanged { get; private set; }
    public bool HasError { get; private set; }
    public object? CurrentValue => _property.GetValue(_model);
    public IReadOnlyList<string> ErrorList => _listError;

    public void NotifyChange()
    {
        object? currentValue = CurrentValue;

        if (Equals(currentValue, _originalValue)) return;
        HasChanged = true;
        Validate();
        Changed?.Invoke(this);
    }

    public void ClearErrorList()
    {
        HasError = false;
        _listError.Clear();
        Changed?.Invoke(this);
    }

    public void Reset()
    {
        HasChanged = false;

        if (_property.CanWrite)
        {
            _property.SetValue(_model, _originalValue);
        }

        ClearErrorList();
    }

    public bool Validate()
    {
        ClearErrorList();

        object? value = CurrentValue;

        ValidationContext context = new(_model)
        {
            MemberName = Name
        };

        List<ValidationResult> results = [];
        bool isValid = Validator.TryValidateProperty(value, context, results);

        foreach (ValidationResult result in results)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                _listError.Add(result.ErrorMessage);
            }
        }

        HasError = !isValid;

        Validated?.Invoke(this);
        return isValid;
    }

    public void WriteError(string message)
    {
        AddError(message);
        HasError = true;
        Changed?.Invoke(this);
    }

    public void WriteError(params string[] list)
    {
        foreach (string message in list)
        {
            AddError(message);
        }

        HasError = true;
    }

    public event IWebFormFieldContext.WebFormFieldContextEventHandler? Validated;
    public event Action<WebFormFieldContext>? Changed;

    public void AddError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _listError.Add(message);
        Changed?.Invoke(this);
    }
}

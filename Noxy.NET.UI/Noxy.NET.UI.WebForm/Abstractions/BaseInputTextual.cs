using System.Globalization;
using Microsoft.AspNetCore.Components;
using Noxy.NET.Interfaces;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputTextual<TValue> : BaseInput<TValue>, IBaseInputTextual
{
    [Inject]
    protected IDebouncerService DebouncerService { get; set; } = null!;

    [Parameter]
    public int? Size { get; set; }
    protected int SizeCurrent => Size ?? 40;

    [Parameter]
    public int? OnInputDelay { get; set; }

    protected string ValueInternal { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ValueInternal = GetCurrentValue();
    }

    protected virtual string GetCurrentValue()
    {
        return Value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => Value.ToString() ?? string.Empty
        };
    }

    protected virtual void OnValueInput(ChangeEventArgs args)
    {
        if (!OnInputDelay.HasValue) return;

        Console.WriteLine(args.Value);
        string value = GetChangeEventArgsValue(args);
        if (OnInputDelay.Value > 0)
        {
            DebouncerService.Debounce(() => HandleChange(value), new(OnInputDelay.Value, true, UUIDString));
        }
        else
        {
            HandleChange(value);
        }
    }

    protected virtual void OnValueChange(ChangeEventArgs args)
    {
        OnChange.InvokeAsync(args);
        HandleChange(GetChangeEventArgsValue(args));
    }

    protected virtual void HandleChange(string value)
    {
        ValueInternal = value;
        NotifyChange(GetParsedValue(value));
    }

    protected abstract TValue? GetParsedValue(string value);

    private static string GetChangeEventArgsValue(ChangeEventArgs args)
    {
        return args.Value?.ToString() ?? string.Empty;
    }
}

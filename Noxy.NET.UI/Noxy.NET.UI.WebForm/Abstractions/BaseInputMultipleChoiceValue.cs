using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputMultipleChoiceValue<TOption, TValue> : BaseInputValue<TValue> where TValue : IEnumerable<TOption>
{
    [Parameter, EditorRequired]
    public required IEnumerable<TOption> OptionList { get; set; }

    [Parameter]
    public string? Name { get; set; }
    protected string NameCurrent => Name ?? UUIDCode;

    [Parameter]
    public Func<TOption, IReadOnlyDictionary<string, object>?>? InputAttributes { get; set; }

    protected void OnInputChange(ChangeEventArgs args, TOption option)
    {
        TOption? value = GetEventValue(args) ? option : default;
        List<TOption> current = Value.ToList();
        List<TOption> next = [];
        if (IsEqual(value, default))
        {
            next.AddRange(current.Where(x => !IsEqual(x, option)));
        }
        else
        {
            next.AddRange(current);
            next.Add(option);
        }

        TValue result = (TValue)Activator.CreateInstance(typeof(TValue), next.AsEnumerable())!;
        NotifyChange(result);
    }

    protected bool IsChecked(TOption option)
    {
        return Value.Any(x => IsEqual(x, option));
    }

    private static bool IsEqual(TOption? a, TOption? b)
    {
        return EqualityComparer<TOption>.Default.Equals(a, b);
    }

    private static bool GetEventValue(ChangeEventArgs args)
    {
        if (args.Value is bool parsed) return parsed;
        return bool.TryParse(args.Value?.ToString(), out bool value) && value;
    }
}

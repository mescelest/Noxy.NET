using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputMultipleChoice<TOption, TValue> : BaseInput<TValue> where TValue : IEnumerable<TOption>
{
    [Parameter, EditorRequired]
    public required IEnumerable<TOption> OptionList { get; set; }

    [Parameter]
    public Func<TOption, IReadOnlyDictionary<string, object>?>? InputAttributes { get; set; }

    private static readonly ConstructorInfo? EnumerableConstructor = typeof(TValue).GetConstructor([typeof(IEnumerable<TOption>)]);
    private static readonly bool IsListAssignable = typeof(TValue).IsAssignableFrom(typeof(List<TOption>));
    private static readonly bool IsArrayAssignable = typeof(TValue).IsAssignableFrom(typeof(TOption[]));

    protected void OnInputChange(ChangeEventArgs args, TOption option)
    {
        bool isChecked = GetEventValue(args);
        HashSet<TOption> set = Value != null ? [..Value] : [];

        if (isChecked)
        {
            set.Add(option);
        }
        else
        {
            set.Remove(option);
        }

        TValue result = CreateValue(set.ToList());
        NotifyChange(result);
    }

    private static TValue CreateValue(List<TOption> items)
    {
        if (EnumerableConstructor is not null) return (TValue)EnumerableConstructor.Invoke([items]);
        if (IsListAssignable) return (TValue)(object)items;
        if (IsArrayAssignable) return (TValue)(object)items.ToArray();

        throw new InvalidOperationException($"TValue '{typeof(TValue).Name}' is not supported. It must have a constructor accepting IEnumerable<TOption>.");
    }

    protected bool IsChecked(TOption option)
    {
        return Value is not null && Value.Any(x => IsEqual(x, option));
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

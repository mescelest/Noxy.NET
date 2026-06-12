using Microsoft.AspNetCore.Components;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseInputMultipleChoice<TOption, TValue> : BaseInput<TValue>, INotifyInputChanged where TValue : ISet<TOption>, new()
{
    [Parameter, EditorRequired]
    public required HashSet<TOption> OptionList { get; set; }

    [Parameter]
    public Func<TOption, IReadOnlyDictionary<string, object>?>? InputAttributes { get; set; }

    [Parameter]
    public EventCallback<ChangeEventArgs> OnChange { get; set; }

    protected void OnChoiceChange(ChangeEventArgs args, TOption option)
    {
        bool isChecked = GetEventValue(args);
        TValue set = Value ?? [];

        if (isChecked)
        {
            set.Add(option);
        }
        else
        {
            set.Remove(option);
        }

        OnChange.InvokeAsync(args);
        NotifyChange(set);
    }

    protected bool IsChecked(TOption option) => Value?.Contains(option) == true;

    private static bool GetEventValue(ChangeEventArgs args)
    {
        if (args.Value is bool b) return b;
        return bool.TryParse(args.Value?.ToString(), out bool parsed) && parsed;
    }
}

using Microsoft.AspNetCore.Components;

namespace Noxy.NET.UI.Abstractions;

public abstract class ElementComponent : BlazorComponent
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; init; }

    protected override string CssClass => CombineCssClass(base.CssClass, GetComponentClass());

    protected void InvokeEventCallback<TArgs>(string param, TArgs args) where TArgs : EventArgs
    {
        if (!(AdditionalAttributes?.TryGetValue(param, out object? objFunction) ?? false)) return;

        switch (objFunction)
        {
            case Action<TArgs> fnAction:
                fnAction(args);
                break;
            case EventCallback<TArgs> fnCallback:
                fnCallback.InvokeAsync(args);
                break;
        }
    }

    protected virtual string GetComponentClass()
    {
        List<string> result = ["Component"];
        if (TryExtractAttribute("class", out string? @class) && !string.IsNullOrWhiteSpace(@class))
        {
            result.AddRange(@class.Split(' '));
        }

        return CombineCssClass([..result]);
    }

    protected bool TryExtractAttribute<T>(string attribute, out T? result)
    {
        return TryExtractAttribute(AdditionalAttributes, attribute, out result);
    }

    protected static bool TryExtractAttribute<T>(IReadOnlyDictionary<string, object>? collection, string attribute, out T? result)
    {
        if (collection is not null && collection.TryGetValue(attribute, out object? value) && value.GetType() == typeof(T))
        {
            result = (T)value;
            return true;
        }

        result = default;
        return false;
    }
}

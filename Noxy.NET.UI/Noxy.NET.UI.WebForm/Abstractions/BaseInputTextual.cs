using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
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

    private static readonly Func<string, TValue?> _parser = BuildParser();

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
        var parsed = GetParsedValue(value);
        NotifyChange(GetParsedValue(value));
    }

    protected virtual TValue? GetParsedValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value) ? _parser(value) : default;
    }


    private static string GetChangeEventArgsValue(ChangeEventArgs args)
    {
        return args.Value?.ToString() ?? string.Empty;
    }

    private static Func<string, TValue?> BuildParser()
    {
        Type type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
        MethodInfo? tryParse = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, null, [typeof(string), typeof(IFormatProvider), type.MakeByRefType()], null);
        if (tryParse is null) return _ => default;

        ParameterExpression value = Expression.Parameter(typeof(string), "value");
        ParameterExpression result = Expression.Variable(type, "result");
        ConstantExpression @null = Expression.Constant(null, typeof(TValue?));
        LabelTarget label = Expression.Label(typeof(TValue?));

        MethodCallExpression call = Expression.Call(tryParse, value, Expression.Constant(CultureInfo.InvariantCulture), result);
        ConditionalExpression ternary = Expression.IfThenElse(call, Expression.Return(label, Expression.Convert(result, typeof(TValue?))), Expression.Return(label, @null));
        BlockExpression block = Expression.Block([result], ternary, Expression.Label(label, @null));

        return Expression.Lambda<Func<string, TValue?>>(block, value).Compile();
    }
}

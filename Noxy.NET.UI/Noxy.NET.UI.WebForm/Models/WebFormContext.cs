using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class WebFormContext<TModel>(TModel value) : IDisposable, IWebFormContext<TModel> where TModel : class
{
    protected Dictionary<string, IWebFormFieldContext> WebFormFieldContextCollection { get; } = [];

    protected List<string> ErrorList { get; } = [];
    protected ValidationContext ValidationContext { get; } = new(value);
    protected List<ValidationResult> ValidationResult { get; } = [];

    public void Dispose()
    {
        foreach (KeyValuePair<string, IWebFormFieldContext> pair in WebFormFieldContextCollection)
        {
            pair.Value.Validated -= OnFieldValidated;
        }

        GC.SuppressFinalize(this);
    }

    public TModel Model { get; } = value;
    public bool HasError { get; private set; }
    public bool IsSubmitting { get; private set; }

    public event IWebFormContext.WebFormContextEventHandler? ContextChanged;
    public event IWebFormContext.WebFormContextEventHandler? ContextValidated;

    public IWebFormFieldContext? GetField<T>(Expression<Func<T>>? expression)
    {
        return GetField(GetExpressionFieldName(expression));
    }

    public IWebFormFieldContext? GetField(string name)
    {
        if (Model.GetType().GetProperty(name) == null)
        {
            return null;
        }

        if (WebFormFieldContextCollection.TryGetValue(name, out IWebFormFieldContext? field))
        {
            return field;
        }

        WebFormFieldContextCollection[name] = new WebFormFieldContext(name, Model);
        WebFormFieldContextCollection[name].Validated += OnFieldValidated;
        return WebFormFieldContextCollection[name];
    }

    public string[] GetFormErrorList()
    {
        return ErrorList.ToArray();
    }

    public string[] GetFieldErrorList()
    {
        return WebFormFieldContextCollection.SelectMany(x => x.Value.GetErrorList()).ToArray();
    }

    public bool GetFormHasChanged()
    {
        return WebFormFieldContextCollection.Any(x => x.Value.HasChanged);
    }

    public bool Validate()
    {
        Clear();

        ValidationResult.Clear();
        bool result = Validator.TryValidateObject(Model, ValidationContext, ValidationResult, true);
        foreach (ValidationResult entryValidationResult in ValidationResult)
        {
            if (string.IsNullOrWhiteSpace(entryValidationResult.ErrorMessage)) continue;

            foreach (string name in entryValidationResult.MemberNames)
            {
                IWebFormFieldContext? field = GetField(name);
                if (field != null)
                {
                    field.WriteError(entryValidationResult.ErrorMessage);
                }
                else
                {
                    ErrorList.Add(entryValidationResult.ErrorMessage);
                }
            }
        }

        ContextChanged?.Invoke(this);
        ContextValidated?.Invoke(this);
        return HasError = result;
    }

    public void WriteError(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        ErrorList.Add(message);
        ContextChanged?.Invoke(this);
    }

    public void Clear()
    {
        ErrorList.Clear();

        foreach (KeyValuePair<string, IWebFormFieldContext> item in WebFormFieldContextCollection)
        {
            item.Value.Clear();
        }

        ContextChanged?.Invoke(this);
    }

    public void Reset()
    {
        ErrorList.Clear();

        foreach (KeyValuePair<string, IWebFormFieldContext> item in WebFormFieldContextCollection)
        {
            item.Value.Reset();
        }

        ContextChanged?.Invoke(this);
    }

    public void Submit(IWebFormContext.SubmitAction func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        func();
    }

    public async Task SubmitAsync(IWebFormContext.SubmitActionAsync func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        await func().ConfigureAwait(false);
    }

    public TResult SubmitWithResult<TResult>(IWebFormContext.SubmitAction<TResult> func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        return func();
    }

    public async Task<TResult> SubmitWithResultAsync<TResult>(IWebFormContext.SubmitActionAsync<TResult> func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        return await func().ConfigureAwait(false);
    }

    public void Submit(IWebFormContext<TModel>.SubmitFunc func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        func(this);
    }

    public async Task SubmitAsync(IWebFormContext<TModel>.SubmitFuncAsync func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        await func(this).ConfigureAwait(false);
    }

    public TResult SubmitWithResult<TResult>(IWebFormContext<TModel>.SubmitFunc<TResult> func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        return func(this);
    }

    public async Task<TResult> SubmitWithResultAsync<TResult>(IWebFormContext<TModel>.SubmitFuncAsync<TResult> func)
    {
        using SubmitScope scope = new SubmitScope(this).Start();
        return await func(this).ConfigureAwait(false);
    }

    public void HandleException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        Dictionary<string, IEnumerable<string>> data = [];
        foreach (KeyValuePair<object, object> pair in exception.Data)
        {
            string? key = pair.Key.ToString();
            if (key is null) continue;

            string[] value = pair.Value switch
            {
                string @string => [@string],
                IEnumerable<string> listString => listString.ToArray(),
                IEnumerable<object> listObject => listObject.Select(x => x.ToString()).OfType<string>().ToArray(),
                _ => new[] { pair.Value.ToString() }.OfType<string>().ToArray()
            };

            if (value.Length == 0) continue;
            data[key] = value;
        }

        HandleException(exception.Message, data);
    }

    public void HandleException(string message, Dictionary<string, IEnumerable<string>>? data = null)
    {
        Clear();

        WriteError(message);
        foreach (KeyValuePair<string, IEnumerable<string>> entry in data ?? [])
        {
            GetField(entry.Key)?.WriteError(entry.Value.ToArray());
        }

        ContextChanged?.Invoke(this);
    }

    private void OnFieldValidated(IWebFormFieldContext sender)
    {
        HasError = HasError || sender.HasError;
    }

    private static string GetExpressionFieldName<TResult>(Expression<Func<TResult>>? expression) => expression?.Body is MemberExpression member ? member.Member.Name : string.Empty;

    private class SubmitScope(WebFormContext<TModel> context) : IDisposable
    {
        public void Dispose()
        {
            context.IsSubmitting = false;
        }

        public SubmitScope Start()
        {
            context.IsSubmitting = true;
            return this;
        }
    }
}

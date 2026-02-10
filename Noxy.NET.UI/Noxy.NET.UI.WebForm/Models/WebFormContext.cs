using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class WebFormContext<TModel> : IWebFormContext<TModel>, IDisposable where TModel : class
{
    private bool _batchChanged;

    private int _batchCount;

    public WebFormContext(TModel value)
    {
        Model = value;
        FieldNameList = value.GetType().GetProperties().Select(p => p.Name).ToHashSet();
        foreach (string name in FieldNameList) RegisterField(name);
    }

    public bool IsSubmitting { get; private set; }

    protected Dictionary<string, WebFormFieldContext> WebFormFieldContextCollection { get; } = [];
    protected List<string> ErrorList { get; } = [];

    private bool AnyFieldHasError => WebFormFieldContextCollection.Values.Any(f => f.HasError);
    private bool AnyFieldHasChanged => WebFormFieldContextCollection.Values.Any(f => f.HasChanged);


    public void Dispose()
    {
        foreach (KeyValuePair<string, WebFormFieldContext> pair in WebFormFieldContextCollection)
        {
            pair.Value.Changed -= OnFieldChanged;
            pair.Value.Validated -= OnFieldValidated;
        }

        WebFormFieldContextCollection.Clear();

        GC.SuppressFinalize(this);
    }

    public TModel Model { get; }
    public bool HasError { get; private set; }
    public bool HasChanged { get; private set; }
    public bool HasAnyError => HasError || AnyFieldHasError;
    public bool HasAnyChanges => HasChanged || AnyFieldHasChanged;
    public IReadOnlySet<string> FieldNameList { get; }

    public event Action<IWebFormContext<TModel>>? Changed;


    public void RegisterField(string name)
    {
        if (!FieldNameList.Contains(name)) throw new InvalidOperationException($"Field '{name}' does not exist.");

        if (WebFormFieldContextCollection.ContainsKey(name)) return;

        WebFormFieldContext field = new(name, Model);
        field.Changed += OnFieldChanged;
        field.Validated += OnFieldValidated;
        WebFormFieldContextCollection[name] = field;
    }

    public bool TryRegisterField(string name)
    {
        if (!FieldNameList.Contains(name)) return false;
        if (WebFormFieldContextCollection.ContainsKey(name)) return false;

        RegisterField(name);
        return true;
    }

    public IWebFormFieldContext GetField<T>(Expression<Func<T>>? expression)
    {
        return GetField(GetFieldNameFromExpression(expression));
    }

    public IWebFormFieldContext GetField(string name)
    {
        return TryGetField(name) ?? throw new InvalidOperationException($"Field '{name}' is not registered.");
    }

    public IWebFormFieldContext? TryGetField<T>(Expression<Func<T>>? expression)
    {
        return TryGetField(GetFieldNameFromExpression(expression));
    }

    public IWebFormFieldContext? TryGetField(string name)
    {
        return WebFormFieldContextCollection.GetValueOrDefault(name);
    }

    public IReadOnlyList<string> GetFormErrorList()
    {
        return ErrorList;
    }

    public IReadOnlyList<string> GetFieldErrorList()
    {
        List<string> list = [];
        foreach (KeyValuePair<string, WebFormFieldContext> pair in WebFormFieldContextCollection)
        {
            list.AddRange(pair.Value.ErrorList);
        }

        return list;
    }

    public IReadOnlyList<string> GetFieldErrorList(string name)
    {
        return GetField(name).ErrorList;
    }

    public bool TryGetFieldErrorList(string name, out IReadOnlyList<string> errors)
    {
        IWebFormFieldContext? field = TryGetField(name);
        if (field is null)
        {
            errors = [];
            return false;
        }

        errors = field.ErrorList;
        return true;
    }

    public bool Validate()
    {
        Batch(() =>
        {
            ErrorList.Clear();
            HasError = false;

            foreach (WebFormFieldContext field in WebFormFieldContextCollection.Values)
            {
                field.Validate();
            }

            ValidationContext context = new(Model);
            List<ValidationResult> listResult = [];
            Validator.TryValidateObject(Model, context, listResult, true);

            foreach (ValidationResult result in listResult)
            {
                if (string.IsNullOrWhiteSpace(result.ErrorMessage)) continue;

                bool assignedToField = false;
                foreach (string memberName in result.MemberNames)
                {
                    if (!WebFormFieldContextCollection.TryGetValue(memberName, out WebFormFieldContext? field)) continue;
                    field.WriteError(result.ErrorMessage);
                    assignedToField = true;
                }

                if (!assignedToField) ErrorList.Add(result.ErrorMessage);
            }

            HasError = ErrorList.Count > 0 || AnyFieldHasError;
        });

        return !HasError;
    }

    public bool ValidateFieldList()
    {
        bool isValid = true;
        Batch(() =>
        {
            foreach (WebFormFieldContext field in WebFormFieldContextCollection.Values)
            {
                if (!field.Validate())
                {
                    isValid = false;
                }
            }
        });
        return isValid;
    }

    public bool ValidateField(string name)
    {
        return GetField(name).Validate();
    }

    public bool TryValidateField(string name, out bool result)
    {
        IWebFormFieldContext? field = TryGetField(name);
        if (field is null)
        {
            result = false;
            return false;
        }

        result = field.Validate();
        return true;
    }


    public void WriteError(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        ErrorList.Add(message);
        HasError = true;
        NotifyChanged();
    }

    public void ClearErrorList()
    {
        Batch(() =>
        {
            HasError = false;
            ErrorList.Clear();

            foreach (KeyValuePair<string, WebFormFieldContext> item in WebFormFieldContextCollection)
            {
                item.Value.ClearErrorList();
            }
        });
    }


    public void Reset()
    {
        Batch(() =>
        {
            HasError = false;
            HasChanged = false;
            ErrorList.Clear();

            foreach (KeyValuePair<string, WebFormFieldContext> item in WebFormFieldContextCollection)
            {
                item.Value.Reset();
            }
        });
    }


    public void HandleException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Dictionary<string, IEnumerable<string>> data = [];
        foreach (KeyValuePair<object, object> pair in exception.Data)
        {
            string? key = pair.Key.ToString();
            if (key is null) continue;

            string[] value = pair.Value switch
            {
                string s => new[] { s },
                IEnumerable<string> list => list.ToArray(),
                IEnumerable<object> list => list.Select(x => x.ToString()).OfType<string>().ToArray(),
                _ => pair.Value.ToString() is { } s2 ? new[] { s2 } : Array.Empty<string>()
            };

            if (value.Length == 0) continue;
            data[key] = value;
        }

        HandleException(exception.Message, data);
    }

    public void HandleException(string message, Dictionary<string, IEnumerable<string>>? data = null)
    {
        Batch(() =>
        {
            ClearErrorList();

            WriteError(message);
            foreach (KeyValuePair<string, IEnumerable<string>> entry in data ?? [])
            {
                TryGetField(entry.Key)?.WriteError(entry.Value.ToArray());
            }
        });
    }

    public void BeginBatch()
    {
        _batchCount++;
    }

    public void EndBatch()
    {
        if (_batchCount == 0) return;

        _batchCount--;
        if (_batchCount != 0 || !_batchChanged) return;

        _batchChanged = false;
        Changed?.Invoke(this);
    }

    private void Batch(Action action)
    {
        BeginBatch();
        action();
        EndBatch();
    }

    private void NotifyChanged()
    {
        if (_batchCount > 0)
        {
            _batchChanged = true;
            return;
        }

        Changed?.Invoke(this);
    }


    public static string GetFieldNameFromExpression<T>(Expression<Func<T>>? expression)
    {
        return expression?.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression m } => m.Member.Name,
            _ => throw new InvalidOperationException("Expression must be a simple member access.")
        };
    }


    internal void BeginSubmit()
    {
        if (IsSubmitting) return;
        IsSubmitting = true;
        NotifyChanged();
    }

    internal void EndSubmit()
    {
        if (!IsSubmitting) return;
        IsSubmitting = false;
        NotifyChanged();
    }


    private void OnFieldChanged(WebFormFieldContext sender)
    {
        bool newValue = AnyFieldHasChanged;
        if (newValue == HasChanged) return;
        HasChanged = newValue;
        NotifyChanged();
    }

    private void OnFieldValidated(IWebFormFieldContext sender)
    {
        bool newValue = ErrorList.Count > 0 || AnyFieldHasError;
        if (newValue == HasError) return;
        HasError = newValue;
        NotifyChanged();
    }
}

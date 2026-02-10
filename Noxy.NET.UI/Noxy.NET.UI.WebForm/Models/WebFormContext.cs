using System.ComponentModel.DataAnnotations;
using Noxy.NET.UI.Interfaces;

namespace Noxy.NET.UI.Models;

public class WebFormContext<TModel> : IWebFormContext<TModel>, IDisposable where TModel : class
{
    public TModel Model { get; }
    public bool HasError { get; private set; }
    public bool HasChanged { get; private set; }
    public bool HasAnyError => HasError || AnyFieldHasError;
    public bool HasAnyChanges => HasChanged || AnyFieldHasChanged;
    public bool IsSubmitting { get; private set; }
    public IReadOnlySet<string> FieldNameList { get; }

    protected Dictionary<string, WebFormFieldContext> WebFormFieldContextCollection { get; } = [];
    protected List<string> ErrorList { get; } = [];

    private bool _batchChanged;
    private int _batchCount;

    private bool AnyFieldHasError => WebFormFieldContextCollection.Values.Any(f => f.HasError);
    private bool AnyFieldHasChanged => WebFormFieldContextCollection.Values.Any(f => f.HasChanged);

    public event Action<IWebFormContext<TModel>>? Changed;

    public WebFormContext(TModel value)
    {
        Model = value;
        FieldNameList = value.GetType().GetProperties().Select(p => p.Name).ToHashSet();
        foreach (string name in FieldNameList) RegisterField(name);
    }

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

    public void NotifyFieldChanged(string name)
    {
        if (WebFormFieldContextCollection.TryGetValue(name, out WebFormFieldContext? field))
        {
            field.NotifyChange();
        }
    }

    public string? GetFieldDisplayName(string name)
    {
        return WebFormFieldContextCollection.TryGetValue(name, out WebFormFieldContext? field) ? field.DisplayName : null;
    }

    public string? GetFieldDescription(string name)
    {
        return WebFormFieldContextCollection.TryGetValue(name, out WebFormFieldContext? field) ? field.Description : null;
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
        Batch(() => isValid = WebFormFieldContextCollection.Values.All(x => x.Validate()));
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
                string s => [s],
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

    private IWebFormFieldContext GetField(string name)
    {
        return TryGetField(name) ?? throw new InvalidOperationException($"Field '{name}' is not registered.");
    }

    private IWebFormFieldContext? TryGetField(string name)
    {
        return WebFormFieldContextCollection.GetValueOrDefault(name);
    }

    private void BeginBatch()
    {
        _batchCount++;
    }

    private void EndBatch()
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

    private void OnFieldChanged(WebFormFieldContext sender)
    {
        bool newValue = AnyFieldHasChanged;
        if (newValue == HasChanged) return;
        HasChanged = newValue;
        NotifyChanged();
    }

    private void OnFieldValidated(WebFormFieldContext sender)
    {
        bool newValue = ErrorList.Count > 0 || AnyFieldHasError;
        if (newValue == HasError) return;
        HasError = newValue;
        NotifyChanged();
    }

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
}

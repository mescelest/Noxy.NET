using Noxy.NET.Abstractions;
using Noxy.NET.CaseManagement.Domain.Models;

namespace Noxy.NET.CaseManagement.Presentation.Models;

public class ContextActionField(StateActionField state) : UniversalValue
{
    public string Title { get; private set; } = state.Title;
    public string Description { get; private set; } = state.Description;
    public string[]? ErrorList { get; private set; } = state.ErrorList;
    public override object? Value { get; set; } = state.Value.GetValue();

    public Dictionary<string, ContextActionFieldAttribute> CollectionAttribute { get; } = state.AttributeCollection.ToDictionary(x => x.Key, x => new ContextActionFieldAttribute(x.Value));

    public ContextActionField Apply(StateActionField state)
    {
        Title = state.Title;
        ErrorList = state.ErrorList;
        Description = state.Description;
        Value = state.Value.GetValue();

        foreach (KeyValuePair<string, StateActionFieldAttribute> pair in state.AttributeCollection)
        {
            CollectionAttribute[pair.Key] = CollectionAttribute.TryGetValue(pair.Key, out ContextActionFieldAttribute? value) ? value.Apply(pair.Value) : new(pair.Value);
        }

        return this;
    }
}

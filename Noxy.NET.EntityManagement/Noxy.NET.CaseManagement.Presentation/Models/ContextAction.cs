using Noxy.NET.CaseManagement.Domain.Models;

namespace Noxy.NET.CaseManagement.Presentation.Models;

public class ContextAction(Guid id, StateAction state)
{
    public Guid ID { get; } = id;
    public string Title { get; private set; } = state.Title;
    public string Description { get; private set; } = state.Description;

    public Dictionary<string, bool> CollectionVisibility { get; } = state.FieldCollection.ToDictionary(x => x.Key, x => x.Value.IsActive);
    public Dictionary<string, ContextActionField> CollectionField { get; } = state.FieldCollection.ToDictionary(x => x.Key, x => new ContextActionField(x.Value));

    public ContextAction Apply(StateAction state)
    {
        Title = state.Title;
        Description = state.Description;

        foreach (KeyValuePair<string, StateActionField> pair in state.FieldCollection)
        {
            CollectionVisibility[pair.Key] = pair.Value.IsActive;
            CollectionField[pair.Key] = CollectionField.TryGetValue(pair.Key, out ContextActionField? value) ? value.Apply(pair.Value) : new(pair.Value);
        }

        return this;
    }
}

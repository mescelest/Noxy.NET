using System.Dynamic;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Models;

public class ActionManager
{
    private Dictionary<Guid, EntitySchemaAction> ActionCollection { get; } = [];
    private Dictionary<Guid, IDictionary<string, object?>> DataCollection { get; } = [];
    private Dictionary<Guid, IDictionary<string, object?>> ContextCollection { get; } = [];

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime TimeLastAccess { get; set; } = DateTime.UtcNow;

    public void Register(Guid id, EntitySchemaAction action, Dictionary<string, object?>? context = null)
    {
        TimeCreated = DateTime.UtcNow;
        ActionCollection[id] = action;

        DataCollection[id] = new ExpandoObject();
        foreach (EntityJunctionSchemaActionHasActionStep junctionStep in action.ActionStepList ?? [])
        {
            EntitySchemaActionStep entityStep = junctionStep.Relation ?? throw new InvalidOperationException();
            foreach (EntityJunctionSchemaActionStepHasActionInput junctionInput in entityStep.ActionInputList ?? [])
            {
                EntitySchemaActionInput entityInput = junctionInput.Relation ?? throw new InvalidOperationException();
                DataCollection[id][entityInput.SchemaIdentifier] = null;
            }
        }

        ContextCollection[id] = new ExpandoObject();
        foreach (KeyValuePair<string, object?> item in context ?? [])
        {
            ContextCollection[id][item.Key] = item.Value;
        }
    }

    public void Deregister(Guid id)
    {
        ActionCollection.Remove(id);
        DataCollection.Remove(id);
        ContextCollection.Remove(id);
    }

    public EntitySchemaAction GetAction(Guid id)
    {
        TimeLastAccess = DateTime.UtcNow;
        return ActionCollection.TryGetValue(id, out EntitySchemaAction? value) ? value : throw new InvalidOperationException();
    }

    public IDictionary<string, object?> GetContext(Guid id)
    {
        TimeLastAccess = DateTime.UtcNow;
        return ContextCollection.TryGetValue(id, out IDictionary<string, object?>? value) ? value : throw new InvalidOperationException();
    }

    public IDictionary<string, object?> GetData(Guid id)
    {
        TimeLastAccess = DateTime.UtcNow;
        return DataCollection.TryGetValue(id, out IDictionary<string, object?>? value) ? value : throw new InvalidOperationException();
    }

    public void SetDataValue(Guid id, string identifier, object? value)
    {
        if (!DataCollection.TryGetValue(id, out IDictionary<string, object?>? data)) throw new InvalidOperationException();

        TimeLastAccess = DateTime.UtcNow;
        data[identifier] = value;
    }
}

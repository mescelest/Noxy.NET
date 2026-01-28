using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Application.Services;

public class ApplicationService : IApplicationService
{
    private EntitySchema? Schema { get; set; }

    public void SetSchema(EntitySchema schema)
    {
        Schema = schema;
        // TODO: Do other logic here
    }

    public EntitySchema GetSchema()
    {
        return Schema ?? throw new NullReferenceException();
    }

    public EntitySchemaAction GetSchemaAction(string identifier)
    {
        return GetSchema().ActionList?.Single(x => x.SchemaIdentifier == identifier)
               ?? throw new KeyNotFoundException(identifier);
    }

    public EntitySchemaActionInput GetSchemaActionInput(string identifier)
    {
        return GetSchema().ActionInputList?.Single(x => x.SchemaIdentifier == identifier)
               ?? throw new KeyNotFoundException(identifier);
    }

    public List<EntitySchemaContext> GetSchemaContext()
    {
        return GetSchema().ContextList ?? [];
    }

    public EntitySchemaContext GetSchemaContext(string identifier)
    {
        return GetSchemaContext().Single(x => x.SchemaIdentifier == identifier)
               ?? throw new KeyNotFoundException(identifier);
    }

    public EntitySchemaElement GetSchemaElement(string identifier)
    {
        return GetSchema().ElementList?.Single(x => x.SchemaIdentifier == identifier)
               ?? throw new KeyNotFoundException(identifier);
    }

    public EntitySchemaProperty.Discriminator GetSchemaProperty(string identifier)
    {
        return GetSchema().PropertyList?.Single(x => x.SchemaIdentifier == identifier)
               ?? throw new KeyNotFoundException(identifier);
    }
}

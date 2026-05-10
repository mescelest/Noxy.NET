using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.Database.Frontend;

public class SchemaSeed(DataContext context, TableSchema schema)
{
    public const string ContextMenu = "Context:Menu";

    public const string ElementPizza = "Element:Pizza";
    public const string ElementPasta = "Element:Pasta";
    public const string ElementDrinks = "Element:Drinks";

    public const string PropertyStringName = "Property:String:Name";
    public const string PropertyStringDescription = "Property:String:Description";
    public const string PropertyIntegerPrice = "Property:Integer:Price";
    public const string PropertyBooleanPopular = "Property:Boolean:Popular";

    public async Task<TableSchemaContext> RegisterContext(string constant, Guid idTitle, string note = "", Guid? idDescription = null)
    {
        Guid id = constant.ToDeterministicGuid();

        TableSchemaContext? entity = await context.SchemaContext.FirstOrDefaultAsync(x => x.ID == id);
        if (entity == null)
        {
            entity = new()
            {
                ID = constant.ToDeterministicGuid(),
                SchemaIdentifier = constant,
                Name = constant,
                Note = note,
                TimeCreated = DateTime.UtcNow,
                TitleTextParameterID = idTitle,
                DescriptionTextParameterID = idDescription,
                SchemaID = schema.ID
            };
            context.SchemaContext.Add(entity);
        }

        return entity;
    }
}

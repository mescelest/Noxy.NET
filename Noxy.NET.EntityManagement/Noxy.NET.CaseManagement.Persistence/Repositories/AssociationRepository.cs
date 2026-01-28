using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.CaseManagement.Application.Interfaces.Repositories;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Abstractions;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Associations;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Persistence.Repositories;

public class AssociationRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : BaseRepository(context, mapperE2T, mapperT2E), IAssociationRepository
{
    public async Task ClearActionInputAttribute(Guid idEntity)
    {
        await Context.SchemaActionInputHasAttributeDynamicValue.Where(x => x.EntityID == idEntity).ExecuteDeleteAsync();
        await Context.SchemaActionInputHasAttributeInteger.Where(x => x.EntityID == idEntity).ExecuteDeleteAsync();
        await Context.SchemaActionInputHasAttributeString.Where(x => x.EntityID == idEntity).ExecuteDeleteAsync();
    }

    public async Task<List<EntityAssociationSchemaActionInputHasAttribute>> AssociateActionInputWithAttribute(Guid idEntity, Guid idRelation, IEnumerable<string> list)
    {
        List<EntityAssociationSchemaActionInputHasAttribute> result = [];
        foreach (string item in list)
        {
            result.Add(await Get(new TableAssociationSchemaActionInputHasAttributeString { EntityID = idEntity, RelationID = idRelation, Value = item }, MapperT2E.Map));
        }

        return result;
    }

    public async Task<List<EntityAssociationSchemaActionInputHasAttribute>> AssociateActionInputWithAttribute(Guid idEntity, Guid idRelation, IEnumerable<int?> list)
    {
        List<EntityAssociationSchemaActionInputHasAttribute> result = [];
        foreach (int? item in list)
        {
            result.Add(await Get(new TableAssociationSchemaActionInputHasAttributeInteger { EntityID = idEntity, RelationID = idRelation, Value = item }, MapperT2E.Map));
        }

        return result;
    }

    public async Task<List<EntityAssociationSchemaActionInputHasAttribute>> AssociateActionInputWithAttribute(Guid idEntity, Guid idRelation, IEnumerable<GenericUUID<EntitySchemaDynamicValue>?> list)
    {
        List<EntityAssociationSchemaActionInputHasAttribute> result = [];
        foreach (GenericUUID<EntitySchemaDynamicValue>? item in list)
        {
            result.Add(await Get(new TableAssociationSchemaActionInputHasAttributeDynamicValue { EntityID = idEntity, RelationID = idRelation, ValueID = item?.Value }, MapperT2E.Map));
        }

        return result;
    }

    private async Task<TEntity> Get<TTable, TEntity>(TTable table, Func<TTable, TEntity> mapper) where TTable : TableAssociationSchemaActionInputHasAttribute where TEntity : EntityAssociationSchemaActionInputHasAttribute
    {
        EntityEntry<TTable> entry = await Context.Set<TTable>().AddAsync(table);
        return mapper(entry.Entity);
    }
}

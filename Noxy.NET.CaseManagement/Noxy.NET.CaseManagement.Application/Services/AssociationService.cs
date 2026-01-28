using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.AssociationForms;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Application.Services;

public class AssociationService(IUnitOfWorkFactory serviceUoWFactory) : IAssociationService
{
    public async Task<List<EntityAssociationSchemaActionInputHasAttribute>> Associate(FormModelAssociationSchemaActionInputHasAttribute model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntityAssociationSchemaActionInputHasAttribute> result = model switch
        {
            FormModelAssociationSchemaActionInputHasAttribute<string> value => await uow.Association.AssociateActionInputWithAttribute(model.EntityID, model.RelationID, value.Value),
            FormModelAssociationSchemaActionInputHasAttribute<int?> value => await uow.Association.AssociateActionInputWithAttribute(model.EntityID, model.RelationID, value.Value),
            FormModelAssociationSchemaActionInputHasAttribute<GenericUUID<EntitySchemaDynamicValue>?> value => await uow.Association.AssociateActionInputWithAttribute(model.EntityID, model.RelationID, value.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(model))
        };

        await uow.Commit();

        return result;
    }
}

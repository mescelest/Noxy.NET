using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.AssociationForms;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface IAssociationService
{
    Task<List<EntityAssociationSchemaActionInputHasAttribute>> Associate(FormModelAssociationSchemaActionInputHasAttribute model);
}

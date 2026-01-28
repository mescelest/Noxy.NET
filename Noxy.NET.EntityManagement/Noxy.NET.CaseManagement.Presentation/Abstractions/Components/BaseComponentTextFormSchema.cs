using Microsoft.AspNetCore.Components;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentTextFormSchema<TForm, TEntity> : BaseComponentTextFormTemplate<TForm, TEntity> where TForm : BaseFormModelEntitySchema where TEntity : BaseEntitySchema
{
    [Parameter, EditorRequired]
    public required EntitySchema Schema { get; set; }
}

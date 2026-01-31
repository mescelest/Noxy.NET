using Microsoft.AspNetCore.Components;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentFormSchema<TForm, TEntity> : BaseComponentFormTemplate<TForm, TEntity> where TForm : BaseFormModelEntitySchema where TEntity : BaseEntitySchema
{
    [Parameter, EditorRequired]
    public required EntitySchema Schema { get; set; }
}

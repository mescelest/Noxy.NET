using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentFormTemplate<TForm, TEntity> : BaseComponentFormEntity<TForm, TEntity> where TForm : BaseFormModelEntityTemplate where TEntity : BaseEntityTemplate;

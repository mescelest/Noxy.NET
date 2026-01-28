using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;

namespace Noxy.NET.CaseManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentTextFormTemplate<TForm, TEntity> : BaseComponentTextFormEntity<TForm, TEntity> where TForm : BaseFormModelEntityTemplate where TEntity : BaseEntityTemplate;

using Microsoft.AspNetCore.Components;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Constants;

namespace Noxy.NET.EntityManagement.Presentation.Abstractions.Components;

public abstract class BaseComponentTextFormEntity<TForm, TEntity> : BaseComponentTextForm<TForm> where TForm : BaseFormModelEntity where TEntity : BaseEntity
{
    [Parameter]
    public TEntity? Value { get; set; }

    [Parameter]
    public EventCallback<TEntity> OnChange { get; set; }

    protected string SubmitText => TextService.Get(Context.Model.ID != Guid.Empty ? TextConstants.ButtonUpdate : TextConstants.ButtonCreate);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        await WithLoading(TextService.Resolve);
    }

    protected async void FormSubmit(BaseFormModelEntity model)
    {
        try
        {
            if (!Context.Validate()) return;
            TEntity result = await HandleSubmission(model);
            await OnChange.InvokeAsync(result);
        }
        catch (Exception e)
        {
            Context.HandleException(e);
        }
    }

    protected virtual async Task<TEntity> HandleSubmission(BaseFormModelEntity model)
    {
        return await SchemaAPIService.PostForm<TEntity>(model);
    }
}

namespace Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;

public abstract class BaseViewModelSchema : BaseViewModel
{
    public required string SchemaIdentifier { get; set; }
}

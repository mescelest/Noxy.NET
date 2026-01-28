namespace Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;

public abstract class BaseViewModelSchemaComponent : BaseViewModelSchema
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required int Order { get; set; }
}

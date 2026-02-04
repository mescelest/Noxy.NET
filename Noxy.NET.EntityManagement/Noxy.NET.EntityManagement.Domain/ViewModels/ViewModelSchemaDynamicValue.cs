using Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.EntityManagement.Domain.ViewModels;

public class ViewModelParameter : BaseViewModelSchema
{
    public required string Name { get; set; }
}

using Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.EntityManagement.Domain.ViewModels;

public class ViewModelDataElement : BaseViewModelSchemaComponent
{
    public required IEnumerable<ViewModelDataProperty>? PropertyList { get; set; }
}

using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelDataElement : BaseViewModelSchemaComponent
{
    public required IEnumerable<ViewModelDataProperty>? PropertyList { get; set; }

}

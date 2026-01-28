using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaContext : BaseViewModelSchemaComponent
{

    public List<ViewModelSchemaAction>? ActionList { get; set; }

}

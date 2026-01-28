using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaProperty : BaseViewModelSchemaComponent
{
    public required string DefaultValue { get; set; }
}

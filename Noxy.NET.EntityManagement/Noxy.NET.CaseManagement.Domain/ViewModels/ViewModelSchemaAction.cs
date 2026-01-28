using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaAction : BaseViewModelSchemaComponent
{
    public ViewModelSchemaActionHasActionStep[]? ActionStepList { get; set; }
}

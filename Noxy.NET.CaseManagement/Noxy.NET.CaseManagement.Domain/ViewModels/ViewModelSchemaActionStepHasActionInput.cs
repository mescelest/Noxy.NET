using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaActionStepHasActionInput : BaseViewModel
{
    public required int Order { get; set; }
    public required ViewModelSchemaActionInput? ActionInput { get; set; }
}

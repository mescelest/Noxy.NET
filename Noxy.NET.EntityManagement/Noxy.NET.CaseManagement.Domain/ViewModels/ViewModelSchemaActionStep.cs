using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaActionStep : BaseViewModelSchemaComponent
{
    public ViewModelSchemaActionStepHasActionInput[]? ActionInputList { get; set; }
}

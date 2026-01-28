using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaActionInput : BaseViewModelSchemaComponent
{
    public ViewModelSchemaInput? Input { get; set; } 
    
    public ViewModelSchemaActionInputHasAttribute[]? ActionInputAttributeList { get; set; }
}
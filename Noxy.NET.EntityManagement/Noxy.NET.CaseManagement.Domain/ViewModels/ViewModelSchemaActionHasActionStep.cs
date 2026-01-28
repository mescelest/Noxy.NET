using Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.CaseManagement.Domain.ViewModels;

public class ViewModelSchemaActionHasActionStep : BaseViewModel
{
    public required int Order { get; set; }
    public required ViewModelSchemaActionStep? ActionStep { get; set; }
}

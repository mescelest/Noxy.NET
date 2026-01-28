using Noxy.NET.EntityManagement.Domain.Abstractions.ViewModels;

namespace Noxy.NET.EntityManagement.Domain.ViewModels;

public class ViewModelSchemaProperty : BaseViewModelSchemaComponent
{
    public required string DefaultValue { get; set; }
}

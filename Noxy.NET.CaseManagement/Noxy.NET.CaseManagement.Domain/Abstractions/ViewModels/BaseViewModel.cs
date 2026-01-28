namespace Noxy.NET.CaseManagement.Domain.Abstractions.ViewModels;

public abstract class BaseViewModel
{
    public Guid ID { get; init; } = Guid.NewGuid();
}
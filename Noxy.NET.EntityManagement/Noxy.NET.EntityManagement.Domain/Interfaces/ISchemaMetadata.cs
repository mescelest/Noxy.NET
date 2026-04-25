namespace Noxy.NET.EntityManagement.Domain.Interfaces;

public interface ISchemaMetadata
{
    string Name { get; set; }
    string? Note { get; set; }
}

namespace Noxy.NET.EntityManagement.Domain.Entities;

// An image, held as the reference to where it is stored.
public sealed class DataPropertyImage : DataProperty<string, HistoryPropertyImage> { }
public sealed class HistoryPropertyImage : HistoryProperty<string> { }

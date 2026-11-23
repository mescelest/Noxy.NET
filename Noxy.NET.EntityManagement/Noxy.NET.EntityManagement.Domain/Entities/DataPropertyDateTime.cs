namespace Noxy.NET.EntityManagement.Domain.Entities;

// A point in time, always in UTC.
public sealed class DataPropertyDateTime : DataProperty<DateTime, HistoryPropertyDateTime> { }
public sealed class HistoryPropertyDateTime : HistoryProperty<DateTime> { }

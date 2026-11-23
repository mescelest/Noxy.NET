using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Noxy.NET.EntityManagement.Persistence.Configurations;

// Every time is kept in UTC. SQLite forgets the kind of a DateTime, so it is put back when it is read.
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

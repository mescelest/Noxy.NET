using Noxy.NET.CaseManagement.Persistence;
using Noxy.NET.CaseManagement.Persistence.Tables.Data;

namespace Noxy.NET.CaseManagement.Database.Builders;

public class DataSeedBuilder(DataContext context)
{
    public DateTime Now { get; } = DateTime.UtcNow;

    public TableDataTextParameter AddTextParameter(string identifier, string value, DateTime? timeApproved = null, DateTime? timeEffective = null, DateTime? timeCreated = null)
    {
        return context.DataTextParameter.Add(new()
        {
            SchemaIdentifier = identifier,
            Value = value,
            TimeCreated = timeCreated ?? Now,
            TimeApproved = timeApproved ?? Now,
            TimeEffective = timeEffective ?? Now
        }).Entity;
    }
}

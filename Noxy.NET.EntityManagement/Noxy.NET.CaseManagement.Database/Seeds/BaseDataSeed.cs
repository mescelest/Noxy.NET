using Noxy.NET.CaseManagement.Persistence;

namespace Noxy.NET.CaseManagement.Database.Seeds;

public class BaseDataSeed(DataContext context)
{
    public async Task Apply()
    {
        // DataSeedBuilder builderData = new(context);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

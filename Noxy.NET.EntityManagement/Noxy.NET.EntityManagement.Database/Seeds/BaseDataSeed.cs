using Noxy.NET.EntityManagement.Persistence;

namespace Noxy.NET.EntityManagement.Database.Seeds;

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

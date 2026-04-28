using Noxy.NET.EntityManagement.Persistence;

namespace Noxy.NET.EntityManagement.Database.Seeds;

public class BaseTextSeed(DataContext context)
{
    public async Task Apply()
    {
        Console.WriteLine($"Seeding with {context}");
    }
}

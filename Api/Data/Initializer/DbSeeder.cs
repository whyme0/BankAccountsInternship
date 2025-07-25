using Api.Models;

namespace Api.Data.Initializer
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Clients.Any()) return;

            var client1 = new Client { Id = Guid.NewGuid(), Name = "Anna Manager" };
            var client2 = new Client { Id = Guid.NewGuid(), Name = "Ivan Client" };
            var client3 = new Client { Id = Guid.NewGuid(), Name = "Alex Cashier" };

            context.Clients.AddRange(client1, client2, client3);

            await context.SaveChangesAsync();
        }
    }

}

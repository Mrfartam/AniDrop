using AniDrop.Domain;
using AniDrop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AniDrop.DBInfrastructure;

public static class DBInitializer
{
    public static async Task SeedAsync(AniDropDBContext context, IAuthService authService)
    {
        await context.Database.MigrateAsync();

        if(!await context.Users.AnyAsync(u => u.Username == "test"))
        {
            var testUser = new User
            {
                Username = "test",
                PasswordHash = authService.HashPassword("test"),
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(testUser);
            await context.SaveChangesAsync();
        }
    }

}

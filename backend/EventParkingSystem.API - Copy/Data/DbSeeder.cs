using EventParkingSystem.API.Common;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Data;

public static class DbSeeder
{
    public static async Task SeedDevelopmentAsync(AppDbContext db, IConfiguration configuration, ILogger logger)
    {
        if (!configuration.GetValue<bool>("Seed:Enabled")) return;

        if (!await db.Database.CanConnectAsync())
        {
            logger.LogWarning("Database is not ready. Check LocalDB/DefaultConnection; automatic database creation was attempted.");
            return;
        }

        var email = (configuration["Seed:AdminEmail"] ?? "admin@eventpark.local").Trim().ToLowerInvariant();
        var name = configuration["Seed:AdminName"] ?? "System Admin";
        var password = configuration["Seed:AdminPassword"] ?? "Admin@123";

        // Development-only upsert. This deliberately repairs stale/invalid demo-admin data
        // left behind by an older schema or package so Swagger login is predictable.
        var admin = await db.Customers.FirstOrDefaultAsync(x => x.Email == email);
        if (admin is null)
        {
            admin = new Customer
            {
                FullName = name,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = Roles.Admin,
                Status = CustomerStatuses.Active,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Customers.Add(admin);
        }
        else
        {
            admin.FullName = name;
            admin.PasswordHash = PasswordHasher.Hash(password);
            admin.Role = Roles.Admin;
            admin.Status = CustomerStatuses.Active;
            admin.EmailVerified = true;
            admin.EmailVerificationToken = null;
            admin.EmailVerificationTokenExpiresAt = null;
            admin.PasswordResetToken = null;
            admin.PasswordResetTokenExpiresAt = null;
            admin.UpdatedAt = DateTime.UtcNow;
        }

        if (!await db.EventCategories.AnyAsync())
        {
            db.EventCategories.AddRange(
                new EventCategory { Name = "Concert" },
                new EventCategory { Name = "Sports" },
                new EventCategory { Name = "Conference" },
                new EventCategory { Name = "Workshop" });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Development seed ready. Demo admin: {Email}", email);
    }
}

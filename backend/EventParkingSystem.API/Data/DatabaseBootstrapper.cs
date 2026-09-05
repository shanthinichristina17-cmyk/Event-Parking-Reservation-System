using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Data;

/// <summary>
/// Development bootstrapper. Creates the LocalDB database/schema automatically
/// when it does not exist, then applies predictable demo seed data.
/// This keeps Swagger usable on locked-down company PCs without requiring
/// developers to run SQL scripts before the first launch.
/// </summary>
public static class DatabaseBootstrapper
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseBootstrapper");

        var autoCreate = configuration.GetValue("Database:AutoCreate", environment.IsDevelopment());
        if (!autoCreate)
        {
            logger.LogInformation("Database auto-create is disabled.");
            return;
        }

        try
        {
            var created = await db.Database.EnsureCreatedAsync();
            logger.LogInformation(created
                ? "Database and schema created automatically."
                : "Database already exists; schema bootstrap not required.");

            if (environment.IsDevelopment())
                await DbSeeder.SeedDevelopmentAsync(db, configuration, logger);
        }
        catch (Exception ex)
        {
            // Do not prevent Swagger from starting. Database-backed endpoints and
            // /health/db will report the database problem clearly.
            logger.LogError(ex,
                "Database bootstrap failed. Swagger will still start. Check LocalDB and the DefaultConnection setting.");
        }
    }
}

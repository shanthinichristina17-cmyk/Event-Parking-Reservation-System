using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Data;

/// <summary>
/// Development bootstrapper. Creates LocalDB on first run and applies small,
/// idempotent reservation schema upgrades for existing developer databases.
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

            if (!created)
                await ReservationSchemaUpgrader.ApplyAsync(db);

            logger.LogInformation(created
                ? "Database and schema created automatically."
                : "Database ready and reservation schema upgrade checked.");

            if (environment.IsDevelopment())
                await DbSeeder.SeedDevelopmentAsync(db, configuration, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Database bootstrap failed. Swagger will still start. Check LocalDB and the DefaultConnection setting.");
        }
    }
}

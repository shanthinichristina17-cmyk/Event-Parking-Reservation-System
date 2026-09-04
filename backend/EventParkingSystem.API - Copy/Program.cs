using EventParkingSystem.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event & Parking Reservation System API",
        Version = "v1"
    });
});
builder.Services.AddCors(options => options.AddPolicy("Angular", policy =>
    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await DatabaseBootstrapper.InitializeAsync(app.Services, app.Configuration, app.Environment);

app.UseCors("Angular");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));
app.MapGet("/health/db", async (AppDbContext db, IWebHostEnvironment env) =>
{
    try
    {
        return await db.Database.CanConnectAsync()
            ? Results.Ok(new { status = "ok", database = "connected", utc = DateTime.UtcNow })
            : Results.Json(new { status = "error", database = "disconnected" }, statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "error",
            database = "disconnected",
            details = env.IsDevelopment() ? ex.GetBaseException().Message : null
        }, statusCode: 503);
    }
});

app.Run();

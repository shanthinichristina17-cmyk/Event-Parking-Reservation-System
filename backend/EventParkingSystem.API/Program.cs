using System.Text;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Repositories;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection("Booking"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
    throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event & Parking Reservation System API",
        Version = "v1",
        Description = "ASP.NET Core 8 backend for the Event & Parking Reservation System."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Paste the JWT token returned by /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Create the LocalDB database/schema before hosted reservation jobs start.
// Failure is logged but does not stop Swagger from opening.
await DatabaseBootstrapper.InitializeAsync(app.Services, app.Configuration, app.Environment);

// Company development PCs cannot trust the ASP.NET HTTPS certificate.
// Local development therefore runs on HTTP only. Production can terminate HTTPS
// at the hosting layer/reverse proxy.

app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow })).AllowAnonymous();

app.MapGet("/health/db", async (AppDbContext db, IWebHostEnvironment env) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok(new { status = "ok", database = "connected", utc = DateTime.UtcNow })
            : Results.Json(new
            {
                status = "error",
                database = "disconnected",
                message = "Database connection failed."
            }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "error",
            database = "disconnected",
            message = "Database connection failed.",
            details = env.IsDevelopment() ? ex.GetBaseException().Message : null
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();

// Development diagnostic: proves JWT generation works before testing /api/auth/login.
app.MapGet("/health/jwt", (IJwtService jwtService, IWebHostEnvironment env) =>
{
    if (!env.IsDevelopment())
        return Results.NotFound();

    var sample = new EventParkingSystem.API.Models.Customer
    {
        CustomerId = 1,
        FullName = "JWT Self Test",
        Email = "jwt-self-test@eventpark.local",
        Role = Roles.Admin,
        Status = CustomerStatuses.Active,
        EmailVerified = true
    };

    var token = jwtService.GenerateToken(sample);
    return Results.Ok(new { status = "ok", jwt = "generated", tokenLength = token.Length });
}).AllowAnonymous();

app.Run();

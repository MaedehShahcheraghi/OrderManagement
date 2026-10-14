using Microsoft.OpenApi;
using OrderManagement.Api.Common.Extensions;
using OrderManagement.Api.Endpoints;
using OrderManagement.Api.Security;
using OrderManagement.Application.Extensions;
using OrderManagement.Infrastructure.Identity.Extensions;
using OrderManagement.Infrastructure.Identity.DataSeeders;
using OrderManagement.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:SqlServer is missing. Configure it using " +
        "User Secrets in Development or a secure configuration provider.");

builder.Services.AddApplication()
    .AddInfrastructure(connectionString)
    .AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddApiSecurity();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Management API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT access token returned by /api/auth/login."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1"));
}
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapOrderEndpoints();
app.MapControllers();
app.MapHealthChecks("/health/live").AllowAnonymous();

if (app.Environment.IsDevelopment() &&
    !string.IsNullOrWhiteSpace(app.Configuration["Bootstrap:AdminEmail"]) &&
    !string.IsNullOrWhiteSpace(app.Configuration["Bootstrap:AdminPassword"]) &&
    !string.IsNullOrWhiteSpace(app.Configuration["Bootstrap:UserEmail"]) &&
    !string.IsNullOrWhiteSpace(app.Configuration["Bootstrap:UserPassword"]))
{
    await using var scope = app.Services.CreateAsyncScope();
    long customerId = app.Configuration.GetValue<long>("Bootstrap:CustomerId");
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync(
        app.Configuration["Bootstrap:AdminEmail"]!,
        app.Configuration["Bootstrap:AdminPassword"]!,
        app.Configuration["Bootstrap:UserEmail"]!,
        app.Configuration["Bootstrap:UserPassword"]!,
        customerId,
        app.Lifetime.ApplicationStopping);
}

app.Run();

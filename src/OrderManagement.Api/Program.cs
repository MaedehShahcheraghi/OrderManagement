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
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
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

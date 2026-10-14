using OrderManagement.Api.Common.Extensions;
using OrderManagement.Api.Endpoints;
using OrderManagement.Api.Security;
using OrderManagement.Application.Extensions;
using OrderManagement.Infrastructure.Identity.Extensions;
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

app.Run();

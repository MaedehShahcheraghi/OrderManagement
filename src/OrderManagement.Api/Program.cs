using OrderManagement.Api.Common.Extensions;
using OrderManagement.Api.Security;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Extensions;
using OrderManagement.Infrastructure.Identity.Extensions;
using OrderManagement.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "SqlServer connection string is missing.");
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication().AddInfrastructure(connectionString)
    .AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddApiSecurity();

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health/live");

app.Run();


using OrderManagement.Api.Common.Extensions;
using OrderManagement.Application.Extensions;
using OrderManagement.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "SqlServer connection string is missing.");

builder.Services.AddApplication().
    AddInfrastructure(connectionString);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health/live");

app.Run();


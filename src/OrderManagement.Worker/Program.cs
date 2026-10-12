using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// RabbitMQ consumers and their dependencies will be registered in the messaging phase.
using var host = builder.Build();
await host.RunAsync();

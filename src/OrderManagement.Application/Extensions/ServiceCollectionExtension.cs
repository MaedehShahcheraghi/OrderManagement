using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;


namespace OrderManagement.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(
            TimeProvider.System);
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<
                ConfirmOrderCommand>());


        return services;
    }
}

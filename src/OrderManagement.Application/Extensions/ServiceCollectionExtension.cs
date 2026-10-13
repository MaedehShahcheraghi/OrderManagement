using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Behaviors;
using OrderManagement.Application.Features.Auth.Commands.Login.Validators;
using OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;


namespace OrderManagement.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(
            TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<
            LoginCommandValidator>(ServiceLifetime.Scoped);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                typeof(ServiceCollectionExtension).Assembly);

            configuration.AddOpenBehavior(
                typeof(ValidationBehavior<,>));
        });


        return services;
    }
}

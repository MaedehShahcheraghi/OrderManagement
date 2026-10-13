using FluentValidation;

namespace OrderManagement.Application.Features.Auth.Commands.Login.Validators;

public sealed class LoginCommandValidator
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(254)
            .WithMessage("Email cannot exceed 254 characters.")
            .EmailAddress()
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MaximumLength(256)
            .WithMessage("Password cannot exceed 256 characters.");
    }
}

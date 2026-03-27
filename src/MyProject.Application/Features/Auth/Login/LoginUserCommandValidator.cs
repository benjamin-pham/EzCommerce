using FluentValidation;

namespace MyProject.Application.Features.Auth.Login;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username không được để trống.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password không được để trống.");
    }
}

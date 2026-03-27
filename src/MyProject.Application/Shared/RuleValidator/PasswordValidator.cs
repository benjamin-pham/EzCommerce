using FluentValidation;

namespace MyProject.Application.Shared.RuleValidator;

public sealed class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(x => x)
            .MinimumLength(8).WithMessage("Password phải có ít nhất 8 ký tự.")
            .Matches(@"[a-z]").WithMessage("Password phải có ít nhất 1 chữ thường.")
            .Matches(@"[A-Z]").WithMessage("Password phải có ít nhất 1 chữ hoa.")
            .Matches(@"[0-9]").WithMessage("Password phải có ít nhất 1 chữ số.");
    }
}

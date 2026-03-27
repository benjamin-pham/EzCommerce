using FluentValidation;

namespace MyProject.Application.Shared.RuleValidator;

public sealed class UsernameValidator : AbstractValidator<string>
{
    public UsernameValidator()
    {
        RuleFor(x => x)
            .MinimumLength(6).WithMessage("Username phải có ít nhất 6 ký tự.")
            .MaximumLength(50).WithMessage("Username không được vượt quá 50 ký tự.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Username chỉ được chứa chữ cái và số.");
    }
}

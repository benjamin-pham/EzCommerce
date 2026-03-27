using FluentValidation;

namespace MyProject.Application.Features.Auth.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName không được để trống.")
            .MaximumLength(100).WithMessage("FirstName không được vượt quá 100 ký tự.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName không được để trống.")
            .MaximumLength(100).WithMessage("LastName không được vượt quá 100 ký tự.");

        When(x => x.Email is not null, () =>
            RuleFor(x => x.Email!)
                .EmailAddress().WithMessage("Email không đúng định dạng."));

        When(x => x.Phone is not null, () =>
            RuleFor(x => x.Phone!)
                .Matches(@"^[0-9+]{7,15}$").WithMessage("Phone phải từ 7–15 ký tự, chỉ chứa số và dấu +."));

        When(x => x.Birthday.HasValue, () =>
            RuleFor(x => x.Birthday!.Value)
                .Must(b => b <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Birthday không được là ngày tương lai."));
    }
}

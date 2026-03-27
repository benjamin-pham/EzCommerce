using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MyProject.Application.Features.Auth.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProfileCommandHandler> logger)
    : ICommandHandler<UpdateProfileCommand>
{
    private static readonly Error UserNotFound =
        new("User.NotFound", "Người dùng không tồn tại.");

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserNotFound);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Birthday);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Profile updated for UserId={UserId} at {Timestamp}",
            userContext.UserId,
            DateTime.UtcNow);

        return Result.Success();
    }
}

using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Features.Auth.Login;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyProject.Application.UnitTests.Auth;

public sealed class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _jwtTokenService,
            _unitOfWork,
            NullLogger<LoginUserCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokens()
    {
        var user = User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

        _userRepository.GetByUsernameAsync("nguyenvana", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify("Secret123", "hashedpw").Returns(true);

        _jwtTokenService.GenerateAccessToken(user.Id).Returns("access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token");
        _jwtTokenService.HashToken("refresh-token").Returns("hashed-refresh-token");

        var command = new LoginUserCommand("nguyenvana", "Secret123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.ExpiresIn.Should().Be(86400);
        result.Value.TokenType.Should().Be("Bearer");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownUsername_ReturnsInvalidCredentials()
    {
        _userRepository.GetByUsernameAsync("unknown", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new LoginUserCommand("unknown", "Secret123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsInvalidCredentials()
    {
        var user = User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

        _userRepository.GetByUsernameAsync("nguyenvana", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify("WrongPassword", "hashedpw").Returns(false);

        var command = new LoginUserCommand("nguyenvana", "WrongPassword");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCredentials_SetsRefreshTokenOnUser()
    {
        var user = User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

        _userRepository.GetByUsernameAsync("nguyenvana", Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify("Secret123", "hashedpw").Returns(true);
        _jwtTokenService.GenerateAccessToken(user.Id).Returns("access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token");
        _jwtTokenService.HashToken("refresh-token").Returns("hashed-refresh-token");

        var command = new LoginUserCommand("nguyenvana", "Secret123");

        await _handler.Handle(command, CancellationToken.None);

        user.HashedRefreshToken.Should().Be("hashed-refresh-token");
        user.RefreshTokenExpiresAt.Should().NotBeNull();
        user.RefreshTokenExpiresAt!.Value.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }
}

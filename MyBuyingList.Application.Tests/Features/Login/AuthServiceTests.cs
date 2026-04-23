using MyBuyingList.Application.Common.Exceptions;
using MyBuyingList.Application.Common.Interfaces;
using MyBuyingList.Application.Common.Options;
using MyBuyingList.Application.Features.Login.DTOs;
using MyBuyingList.Application.Features.Login.Services;
using MyBuyingList.Application.Features.Users;
using MyBuyingList.Application.Features.Users.DTOs;
using MyBuyingList.Domain.Constants;
using MyBuyingList.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace MyBuyingList.Application.Tests.Features.Login;

public class AuthServiceTests
{
    private readonly AuthService _sut;
    private readonly IFixture _fixture;
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    
    private readonly IPasswordEncryptionService _passwordEncryptionService =
        Substitute.For<IPasswordEncryptionService>();

    private readonly ILogger<AuthService> _loggerMock = Substitute.For<ILogger<AuthService>>();
    private readonly LockoutOptions _lockoutOptions = new() { MaxFailedAttempts = 5, LockoutDurationMinutes = 15 };

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepositoryMock,
            _passwordEncryptionService,
            Options.Create(_lockoutOptions),
            _loggerMock);

        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<User>(c => c
            .With(x =>
                    x.Email,
                _fixture.Create<MailAddress>().Address));

        _fixture.Customize<CreateUserRequest>(c => c
            .With(x =>
                    x.Email,
                _fixture.Create<MailAddress>().Address));
    }

    #region Authenticate tests
    
    [Fact]
    public async Task Authenticate_ShouldReturnLoginResponse_WhenAuthenticationIsValid()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 0)
            .Create();
        user.UserName = user.UserName.ToLower();
        user.Id = _fixture.Create<int>();

        var attemptingPassword = _fixture.Create<string>();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = attemptingPassword
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(attemptingPassword, user.Password)
            .Returns(true);

        _userRepositoryMock
            .GetUserPoliciesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        _userRepositoryMock
            .GetUserRoleNamesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        //Act
        var result = await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        result.UserId.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName);
        
    }

    [Fact]
    public async Task Authenticate_ShouldThrowsException_WhenUserDoesNotExist()
    {
        //Arrange
        var dto = new LoginRequest
        {
            Password = _fixture.Create<string>(),
            Username = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(dto.Username, CancellationToken.None)
            .ReturnsNull();

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Authenticate_ShouldThrowsException_WhenPasswordsDontMatch()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 0)
            .Create();

        var dto = new LoginRequest
        {
            Password = _fixture.Create<string>(),
            Username = user.UserName
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(false);

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage($"An error occurred when authenticating user {user.UserName.ToLower()}.");
    }
    

    [Fact]
    public async Task Authenticate_ShouldThrowAccountLockedException_WhenAccountIsLocked()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, DateTime.UtcNow.AddMinutes(10))
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<AccountLockedException>();
    }

    [Fact]
    public async Task Authenticate_ShouldThrowWithCorrectMinutesRemaining_WhenAccountIsAlreadyLocked()
    {
        //Arrange
        const int lockoutMinutes = 10;
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, DateTime.UtcNow.AddMinutes(lockoutMinutes))
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert — MinutesRemaining should be within [lockoutMinutes - 1, lockoutMinutes]
        // because Math.Ceiling is used and a negligible amount of time may elapse during the test.
        var exception = await act.Should().ThrowAsync<AccountLockedException>();
        exception.Which.MinutesRemaining.Should().BeInRange(lockoutMinutes - 1, lockoutMinutes);
    }

    [Fact]
    public async Task Authenticate_ShouldThrowWithCorrectMinutesRemaining_WhenAccountIsJustLocked()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, _lockoutOptions.MaxFailedAttempts - 1)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(false);

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert — MinutesRemaining must equal the configured LockoutDurationMinutes exactly.
        var exception = await act.Should().ThrowAsync<AccountLockedException>();
        exception.Which.MinutesRemaining.Should().Be(_lockoutOptions.LockoutDurationMinutes);
    }

    [Fact]
    public async Task Authenticate_ShouldIncrementCounter_WhenWrongPasswordBelowThreshold()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 2)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(false);

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<AuthenticationException>();
        await _userRepositoryMock.Received(1).IncrementFailedLoginAttemptsAsync(user.Id, null, CancellationToken.None);
    }

    [Fact]
    public async Task Authenticate_ShouldLockAccount_WhenWrongPasswordAtThreshold()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, _lockoutOptions.MaxFailedAttempts - 1)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(false);

        //Act
        var act = async () => await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<AccountLockedException>();
        await _userRepositoryMock.Received(1)
            .IncrementFailedLoginAttemptsAsync(user.Id, Arg.Is<DateTime?>(d => d != null), CancellationToken.None);
    }

    [Fact]
    public async Task Authenticate_ShouldResetLockout_WhenCorrectPasswordAfterFailures()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 3)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest
        {
            Username = user.UserName,
            Password = _fixture.Create<string>()
        };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(true);

        _userRepositoryMock
            .GetUserRoleNamesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        //Act
        var result = await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        await _userRepositoryMock.Received(1).ResetLockoutAsync(user.Id, CancellationToken.None);
    }

    [Fact]
    public async Task Authenticate_ShouldReturnMappedPermissions_WhenUserHasPolicies()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 0)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest { Username = user.UserName, Password = _fixture.Create<string>() };

        var policies = _fixture.CreateMany<Policy>(3).ToList();

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(true);

        _userRepositoryMock
            .GetUserPoliciesAsync(user.Id, CancellationToken.None)
            .Returns(policies);

        _userRepositoryMock
            .GetUserRoleNamesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        //Act
        AuthenticateResult result = await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        result.Permissions.Should().BeEquivalentTo(policies.Select(p => p.Name));
    }

    [Fact]
    public async Task Authenticate_ShouldReturnEmptyPermissions_WhenUserHasNoPolicies()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 0)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest { Username = user.UserName, Password = _fixture.Create<string>() };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(true);

        _userRepositoryMock
            .GetUserPoliciesAsync(user.Id, CancellationToken.None)
            .Returns((List<Policy>?)null);

        _userRepositoryMock
            .GetUserRoleNamesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        //Act
        AuthenticateResult result = await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        result.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Authenticate_ShouldSetIsAdminTrue_WhenUserHasAdministratorRole()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 0)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest { Username = user.UserName, Password = _fixture.Create<string>() };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(true);

        _userRepositoryMock
            .GetUserPoliciesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        _userRepositoryMock
            .GetUserRoleNamesAsync(user.Id, CancellationToken.None)
            .Returns([Roles.Administrator]);

        //Act
        AuthenticateResult result = await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        result.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Authenticate_ShouldSetIsAdminFalse_WhenUserLacksAdministratorRole()
    {
        //Arrange
        var user = _fixture.Build<User>()
            .With(u => u.LockoutEnd, (DateTime?)null)
            .With(u => u.FailedLoginAttempts, 0)
            .Create();
        user.UserName = user.UserName.ToLower();

        var dto = new LoginRequest { Username = user.UserName, Password = _fixture.Create<string>() };

        _userRepositoryMock
            .GetActiveUserByUsernameAsync(user.UserName, CancellationToken.None)
            .Returns(user);

        _passwordEncryptionService
            .VerifyPassword(dto.Password, user.Password)
            .Returns(true);

        _userRepositoryMock
            .GetUserPoliciesAsync(user.Id, CancellationToken.None)
            .Returns([]);

        _userRepositoryMock
            .GetUserRoleNamesAsync(user.Id, CancellationToken.None)
            .Returns([Roles.RegularUser]);

        //Act
        AuthenticateResult result = await _sut.AuthenticateAsync(dto, CancellationToken.None);

        //Assert
        result.IsAdmin.Should().BeFalse();
    }

    #endregion

}

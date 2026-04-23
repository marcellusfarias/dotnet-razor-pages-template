using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyBuyingList.Application.Common.Constants;
using MyBuyingList.Application.Common.Exceptions;
using MyBuyingList.Application.Common.Interfaces;
using MyBuyingList.Application.Common.Options;
using MyBuyingList.Application.Features.Login.DTOs;
using MyBuyingList.Application.Features.Users;
using MyBuyingList.Domain.Constants;
using MyBuyingList.Domain.Entities;

namespace MyBuyingList.Application.Features.Login.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordEncryptionService _passwordEncryptionService;
    private readonly LockoutOptions _lockoutOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordEncryptionService passwordEncryptionService,
        IOptions<LockoutOptions> lockoutOptions,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordEncryptionService = passwordEncryptionService;
        _lockoutOptions = lockoutOptions.Value;
        _logger = logger;
    }

    public async Task<AuthenticateResult> AuthenticateAsync(LoginRequest loginDto, CancellationToken cancellationToken)
    {
        var username = loginDto.Username.ToLower();

        User? user = await _userRepository.GetActiveUserByUsernameAsync(username, cancellationToken);
        if (user is null)
        {
            throw new AuthenticationException(username, ErrorMessages.InvalidUsernameOrPassword);
        }

        if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        {
            var minutesRemaining = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            throw new AccountLockedException(username, minutesRemaining);
        }

        bool verified = _passwordEncryptionService.VerifyPassword(loginDto.Password, user.Password);
        if (!verified)
        {
            var newCount = user.FailedLoginAttempts + 1;
            DateTime? lockoutEnd = newCount >= _lockoutOptions.MaxFailedAttempts
                ? DateTime.UtcNow.AddMinutes(_lockoutOptions.LockoutDurationMinutes)
                : null;

            await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id, lockoutEnd, cancellationToken);

            if (lockoutEnd != null)
            {
                throw new AccountLockedException(username, _lockoutOptions.LockoutDurationMinutes);
            }

            throw new AuthenticationException(username,
                $"{ErrorMessages.InvalidUsernameOrPassword} Attempt {newCount} of {_lockoutOptions.MaxFailedAttempts}.");
        }

        if (user.FailedLoginAttempts > 0)
        {
            await _userRepository.ResetLockoutAsync(user.Id, cancellationToken);
        }

        var policies = await _userRepository.GetUserPoliciesAsync(user.Id, cancellationToken) ?? [];
        var permissions = policies.Select(p => p.Name).ToList();

        var roleNames = await _userRepository.GetUserRoleNamesAsync(user.Id, cancellationToken);
        bool isAdmin = roleNames.Contains(Roles.Administrator);

        _logger.LogInformation("User {Username} authenticated successfully", username);

        return new AuthenticateResult(user.Id, user.UserName, permissions, isAdmin);
    }
}

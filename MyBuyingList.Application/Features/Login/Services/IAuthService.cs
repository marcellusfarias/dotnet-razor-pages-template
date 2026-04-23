using MyBuyingList.Application.Features.Login.DTOs;

namespace MyBuyingList.Application.Features.Login.Services;

/// <summary>
/// Provides authentication functionality.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns their identity and permissions for session creation.
    /// </summary>
    /// <param name="loginDto">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="AuthenticateResult"/> containing the user ID, permissions, and admin flag.</returns>
    /// <exception cref="MyBuyingList.Application.Common.Exceptions.AuthenticationException">Thrown when credentials are invalid.</exception>
    /// <exception cref="MyBuyingList.Application.Common.Exceptions.AccountLockedException">Thrown when the account is locked.</exception>
    Task<AuthenticateResult> AuthenticateAsync(LoginRequest loginDto, CancellationToken cancellationToken);
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyBuyingList.Application.Features.Login.DTOs;
using MyBuyingList.Application.Features.Login.Services;
using MyBuyingList.Infrastructure.Authentication.Constants;
using MyBuyingList.Web.Middlewares.RateLimiting;
using AuthenticateResult = MyBuyingList.Application.Features.Login.DTOs.AuthenticateResult;

namespace MyBuyingList.Web.Pages;

[EnableRateLimiting(AuthenticationRateLimiterPolicy.PolicyName)]
public class LoginModel : BasePageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }
    
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        AuthenticateResult result = await _authService.AuthenticateAsync(Input, cancellationToken);

        ClaimsPrincipal principal = BuildPrincipal(result);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        string redirectTarget = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/users";
        return LocalRedirect(redirectTarget);
    }

    private static ClaimsPrincipal BuildPrincipal(AuthenticateResult result)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new(ClaimTypes.Name, result.UserName),
        ];

        foreach (string permission in result.Permissions)
        {
            claims.Add(new (CustomClaims.Permissions, permission));
        }
        
        if (result.IsAdmin)
        {
            claims.Add(new (CustomClaims.IsAdmin, "true"));
        }
        
        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}

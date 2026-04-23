using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.RateLimiting;

namespace MyBuyingList.Web.Middlewares.RateLimiting;

public class AuthenticationRateLimiterPolicy : IRateLimiterPolicy<IPAddress>
{
    public const string PolicyName = "Authentication";
    private readonly CustomRateLimiterOptions _options;

    public AuthenticationRateLimiterPolicy(ILogger<AuthenticationRateLimiterPolicy> logger,
                                   IOptions<CustomRateLimiterOptions> options)
    {
        OnRejected = (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            ctx.HttpContext.Response.Redirect("/login");
            logger.LogWarning("Request rejected by {PolicyName}", nameof(AuthenticationRateLimiterPolicy));
            return ValueTask.CompletedTask;
        };
        _options = options.Value;
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    public RateLimitPartition<IPAddress> GetPartition(HttpContext httpContext)
    {
        // RemoteIpAddress is null in TestServer (no real TCP connection)
        IPAddress ipAddress = httpContext.Connection.RemoteIpAddress!;

        return RateLimitPartition.GetFixedWindowLimiter(ipAddress,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = _options.PermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _options.QueueLimit,
                Window = TimeSpan.FromSeconds(_options.Window)
            });
    }
}

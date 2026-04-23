## MODIFIED Requirements

### Requirement: Login rate limiting by IP address
The system SHALL apply a fixed-window rate limiter to the login route to mitigate brute-force attacks. The rate limiter SHALL partition by client IP address. The permit limit, queue limit, and window duration SHALL be configurable via `CustomRateLimiterOptions` in `appsettings.json`.

The rate limiter SHALL be applied to the POST handler of the `/login` Razor Page (previously `/api/auth` and `/api/auth/refresh` REST endpoints).

#### Scenario: Login within rate limit
- **WHEN** a client submits login attempts within the configured permit limit
- **THEN** all attempts are processed normally

#### Scenario: Login rate limit exceeded
- **WHEN** a client exceeds the configured permit limit within the window period
- **THEN** the system returns a 429 Too Many Requests response and does NOT process additional login attempts until the window resets

#### Scenario: Rate limit partitioned per IP
- **WHEN** two different IP addresses each exceed the permit limit independently
- **THEN** each IP is rate limited independently without affecting the other

## REMOVED Requirements

### Requirement: Token refresh rate limiting
**Reason**: The `/api/auth/refresh` endpoint and refresh token mechanism are removed. Session renewal is handled automatically by cookie sliding expiration.
**Migration**: No migration required. The `AuthenticationRateLimiterPolicy` is repurposed for the login page only.

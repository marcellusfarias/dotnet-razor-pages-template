## Why

This project is a WebAPI template that serves JSON endpoints consumed by an external client. We want to transform it into a self-contained Razor Pages application where the UI is embedded directly in the same project — eliminating the need for a separate frontend client, simplifying deployment, and enabling server-side rendering with secure HttpOnly cookie authentication.

## What Changes

- **BREAKING** Remove all REST API controllers (`AuthController`, `UsersController`) — Razor Pages replace them entirely
- **BREAKING** Replace JWT bearer token authentication with HttpOnly cookie authentication using ASP.NET Core cookie middleware and sliding expiration
- **BREAKING** Remove refresh token mechanism — cookie sliding expiration handles session renewal automatically
- Remove Swagger/OpenAPI — no API endpoints remain
- Add Razor Pages: Login page, Users list page, User detail/create/edit/delete pages
- Add anti-forgery (CSRF) protection on all form submissions
- Adapt rate limiting from API endpoints to Razor Pages login route
- Retain CorrelationId middleware (valuable for all server-rendered request tracing)
- Update `Program.cs` / `Services.cs` to remove API/JWT configuration and add cookie auth + Razor Pages
- Update `Dockerfile` and `docker-compose` files to reflect the changed entry point and health check behavior
- Update integration tests to exercise Razor Pages + cookie auth instead of API + JWT

## Capabilities

### New Capabilities
- `cookie-auth`: HttpOnly cookie-based authentication and session management with sliding expiration, replacing JWT/refresh-token flow
- `razor-pages-ui`: Server-rendered Razor Pages for Login, Users list, User detail, Create, Edit, and Delete — calling application services directly

### Modified Capabilities
- `rate-limiting`: Rate limiting policy moves from `/api/auth` API endpoints to the `/login` Razor Page POST handler; partition key and window stay the same

## Impact

- **Removed**: `MyBuyingList.Web/Controllers/`, JWT infrastructure (`JwtProvider`, `JwtBearerOptionsSetup`, refresh token services and background cleanup), Swagger configuration, API route configuration
- **Added**: `MyBuyingList.Web/Pages/` with Razor Pages and PageModel classes; cookie auth middleware and options; anti-forgery token handling
- **Modified**: `ConfigureServices.cs` / `Services.cs`, `ConfigureApp.cs`, `Program.cs`, `appsettings.json` (remove JwtSettings, add CookieAuthOptions), `Dockerfile`, `docker-compose.development.yml`, `docker-compose.production.yml`
- **Modified**: Integration tests in `MyBuyingList.Web.Tests` — replace JWT token pre-loading with cookie-based login flow
- **Dependencies changed**: Remove JWT NuGet packages; Razor Pages support is built into `Microsoft.AspNetCore.App` metapackage (no new packages needed)

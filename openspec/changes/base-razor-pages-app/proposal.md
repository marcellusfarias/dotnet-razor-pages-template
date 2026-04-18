## Why

This template currently provides only a .NET Web API backend. For teams that prefer server-rendered UIs over SPAs, the template needs a companion Razor Pages frontend that's pre-wired to the existing API's auth and user features — offering a full-stack .NET-only alternative to the React option.

## What Changes

- Add a `MyBuyingList.Web` ASP.NET Core Razor Pages project under the existing solution
- Pre-configure an HTTP client pointing to the .NET backend API (configurable base URL via options)
- Include authentication flow: login page, cookie-based session using JWT obtained from the API
- Include a basic users list page that calls the existing `GET /api/users` endpoint
- Add a health check status display on the home/dashboard page
- Provide Docker Compose entry so the Razor Pages app runs alongside the API in development

## Capabilities

### New Capabilities
- `razor-app-scaffold`: Base ASP.NET Core Razor Pages project structure with routing, layout, and shared `_Layout.cshtml`; wired into the existing solution
- `api-client`: Typed HTTP client (using `IHttpClientFactory`) wired to the backend API, with JWT attach and automatic token refresh on 401
- `auth-ui`: Login page and cookie-based auth flow that exchanges credentials against the API's `/api/auth/login` endpoint and stores the JWT in an encrypted cookie session
- `users-ui`: Basic users list Razor Page consuming `GET /api/users`
- `dev-proxy`: Docker Compose service entry and `launchSettings.json` profile for the Razor Pages app

### Modified Capabilities

## Impact

- New `MyBuyingList.Web` project added to `MyBuyingList.sln`
- `.github/workflows/deploy.yml` may need a new job for building/deploying the Razor Pages app
- `docker-compose.yml` (if it exists) will be updated to include the frontend service
- No changes to existing API contracts, database schema, or core .NET backend code

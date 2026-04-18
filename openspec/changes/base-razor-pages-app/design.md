## Context

The existing template exposes a .NET Web API with JWT authentication (access + refresh tokens), a users resource, and a health endpoint. There is no server-rendered UI. Developers who want a .NET-only full-stack starting point (no JavaScript build tooling) must wire one up from scratch every time.

This design adds a `MyBuyingList.Web` ASP.NET Core Razor Pages project to the existing solution. The project is deliberately thin — it demonstrates how to integrate a server-rendered UI with the existing backend API, not how to build a full product.

## Goals / Non-Goals

**Goals:**
- Scaffold a working ASP.NET Core Razor Pages project (`MyBuyingList.Web`) added to `MyBuyingList.sln`
- Provide a typed HTTP client (via `IHttpClientFactory`) that attaches JWT tokens and handles refresh automatically
- Provide login/logout Razor Pages backed by the real `/api/auth` endpoints; store tokens in an encrypted session cookie
- Provide a users list Razor Page backed by the real `/api/users` endpoint
- Add a Docker Compose service for the Razor Pages app

**Non-Goals:**
- Production-ready UI/UX design (no CSS framework, no accessibility audit)
- Full CRUD for users (read-only list is sufficient as a pattern demo)
- A separate frontend build pipeline (everything is standard .NET)
- End-to-end tests (existing .NET integration tests cover the API; Razor Pages integration tests are out of scope for a minimal template)
- Blazor or HTMX (Razor Pages is the simplest, most widely understood ASP.NET Core server-rendered option)

## Decisions

### 1. Razor Pages over MVC
Razor Pages co-locates the page model and the view in one feature folder, reducing ceremony for simple CRUD-style pages. MVC adds no value at this scale.

### 2. Cookie-based session for token storage
Alternatives considered:
- **HttpOnly encrypted session cookie** → tokens never exposed to JavaScript, safe from XSS — **chosen**
- **localStorage (JS)** → XSS risk; only viable with a JS frontend
- **In-memory server-side session** → works but breaks horizontal scaling; cookie-based is stateless

The login page posts credentials to the API's `/api/auth/login`, receives access + refresh tokens, and stores them in an ASP.NET Core Data Protection-encrypted cookie (`CookieAuthenticationOptions`). The middleware re-hydrates the tokens from the cookie on every request.

### 3. API communication: `IHttpClientFactory` typed client
A `BackendApiClient` typed client is registered via `AddHttpClient<BackendApiClient>`. A `DelegatingHandler` reads the access token from `IHttpContextAccessor` and attaches it as `Authorization: Bearer`. On a 401 response, the handler calls `POST /api/auth/refresh` with the refresh token from the cookie, updates the cookie, and retries the original request once.

### 4. Project layout
```
MyBuyingList.Web/
  Pages/
    Auth/
      Login.cshtml + Login.cshtml.cs
    Users/
      Index.cshtml + Index.cshtml.cs
    Shared/
      _Layout.cshtml
    Index.cshtml + Index.cshtml.cs   # dashboard / health status
  Api/
    BackendApiClient.cs
    AuthTokenHandler.cs
  Configuration/
    BackendOptions.cs
  wwwroot/                           # static assets
  Program.cs
  Services.cs
  appsettings.json                   # BackendApi:BaseUrl (default: http://localhost:5000)
  appsettings.Development.json
  Dockerfile                         # multi-stage: build → runtime
```

### 5. Docker Compose
A new `web` service builds from `MyBuyingList.Web/Dockerfile` and depends on the existing `api` service. Dev workflow: `docker compose up`.

### 6. Configuration
`BackendOptions` typed options class binds `BackendApi:BaseUrl`. Override via `BackendApi__BaseUrl` environment variable in Docker Compose / production.

## Risks / Trade-offs

- **Cookie size limit (4 KB)** → access + refresh tokens are JWTs; combined size may approach the limit for large payloads. Mitigation: tokens in this template are small; a comment flags the limit for production evaluators.
- **Single-retry refresh loop** → if the refresh token is expired, the handler clears the cookie and redirects to `/auth/login` without infinite looping. Must be manually tested.
- **Cross-origin not applicable** → Razor Pages serves the UI and calls the API server-to-server, so CORS is not a concern for the web project itself.

## Migration Plan

No migration needed — purely additive. The existing API project and solution are unaffected.

To adopt as a template user:
1. `cd MyBuyingList.Web && dotnet run` (or `dotnet run --project MyBuyingList.Web`)
2. Set `BackendApi__BaseUrl` to the running API address if not using Docker Compose defaults.

## Open Questions

- Should the Dockerfile use `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` for smaller image size? (Prefer alpine unless native dependencies are required — yes, use alpine.)
- Should the users list page support pagination? (Out of scope for a minimal template — use whatever the API returns.)

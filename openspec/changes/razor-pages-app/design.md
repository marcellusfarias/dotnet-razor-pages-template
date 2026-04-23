## Context

The project is currently a headless REST WebAPI with JWT bearer authentication, refresh token rotation, and Swagger documentation. The goal is to transform it into a server-rendered Razor Pages application embedded in the same `MyBuyingList.Web` project, with no separate frontend client.

Existing layers (Domain, Application, Infrastructure) remain unchanged. Only the Web presentation layer and its supporting configuration are replaced. The application services, repositories, validators, domain entities, and database remain as-is.

## Goals / Non-Goals

**Goals:**
- Replace `Controllers/` with `Pages/` using ASP.NET Core Razor Pages
- Replace JWT bearer auth with HttpOnly cookie auth (sliding expiration)
- Remove refresh token mechanism — cookie middleware handles renewal
- Remove Swagger and all OpenAPI configuration
- Retain CorrelationId and rate limiting middleware (adapted to new routes)
- Keep anti-forgery (CSRF) protection on all form POSTs (built into Razor Pages by default)
- Update Docker files and integration tests to match the new architecture

**Non-Goals:**
- Changing Domain, Application, or Infrastructure layers
- Adding a CSS framework or JS build pipeline
- Building a rich SPA-like experience (this is a minimal server-rendered template)
- Supporting API consumers alongside the Razor Pages UI

## Decisions

### 1. Cookie Auth via ASP.NET Core Cookie Middleware

**Decision:** Use `services.AddAuthentication().AddCookie(...)` with `HttpOnly = true`, `SameSite = Strict`, and sliding expiration instead of JWT + refresh tokens.

**Rationale:** For a server-rendered app, cookies are the standard, secure mechanism. They eliminate the need for the client to store or manage tokens. Sliding expiration removes the need for explicit refresh endpoint logic. `SameSite = Strict` provides CSRF protection in addition to anti-forgery tokens.

**Alternatives considered:**
- *Keep JWT, adapt to cookies:* Would mean storing a JWT inside the cookie — unnecessary complexity; the cookie itself is the session carrier.
- *Session middleware with server-side store:* Adds infrastructure dependency (distributed cache). Not appropriate for a lean template.

### 2. Remove Refresh Token Infrastructure

**Decision:** Delete `RefreshToken` entity, `RefreshTokenCleanupService`, `RefreshTokenOptions`, and all related code paths.

**Rationale:** Refresh tokens exist to address the short TTL of JWT access tokens. With cookie sliding expiration, the cookie TTL resets on each authenticated request — there is no need for a separate rotation mechanism. Removing this code reduces surface area, attack vectors, and complexity.

**Migration approach:** Drop all existing EF Core migrations and recreate them from scratch to reflect the final schema. There is no production database to preserve.

### 3. Razor Pages Calling Application Services Directly

**Decision:** PageModel classes inject and call Application layer services directly (same as controllers did). No intermediate API layer.

**Rationale:** There are no external consumers. An API layer would be pure overhead. The Application layer's service interfaces serve as the boundary — PageModels are just a different type of presenter.

### 4. Permission Model Retained, Adapted for Pages

**Decision:** Keep the existing claims-based permission system. PageModel handlers use `[Authorize(Policy = Policies.UserGet)]` attributes (or check `User.HasClaim(...)` inline) rather than `[HasPermission]` controller attributes.

**Rationale:** The permission model encodes meaningful business rules (e.g., only admins can delete users). It would be wrong to discard this. The custom `PermissionAuthorizationHandler` and policy provider continue to work with cookie auth because they operate on `ClaimsPrincipal`, which is auth-mechanism-agnostic.

### 5. Rate Limiting Adapted to Login Page Route

**Decision:** Move `AuthenticationRateLimiterPolicy` from `/api/auth` to the `/login` Razor Page POST handler. Policy name, window, and partition key (IP address) remain unchanged.

**Rationale:** The threat model is identical — brute-force credential stuffing against the login form. Only the route changes.

### 6. Anti-Forgery Tokens

**Decision:** Rely on Razor Pages' built-in anti-forgery integration. No extra configuration needed — `@Html.AntiForgeryToken()` is injected in forms and validated by the `[ValidateAntiForgeryToken]` filter automatically on POST handlers when using the Razor Pages tag helpers.

**Rationale:** Built-in, zero-cost CSRF protection.

### 7. Minimal HTML, No CSS Framework

**Decision:** Pages use semantic HTML with minimal inline styles or a simple `<style>` block. No Bootstrap, Tailwind, or JS build pipeline.

**Rationale:** This is a backend template. Consumers add their own styling. Keeping it framework-free avoids CDN dependencies and keeps the template lean.

### 8. Health Check Route Unchanged

**Decision:** Retain `GET /health` anonymous endpoint via `app.MapHealthChecks("/health")`.

**Rationale:** Docker health checks in docker-compose reference this endpoint. No change needed.

## Risks / Trade-offs

- **Migrations recreated from scratch** → All existing EF Core migrations are deleted and a single new initial migration is created from the current model (without `RefreshTokens`). No production database exists, so there is no data-loss risk.
- **Integration test rewrite** → Tests that pre-load a JWT will need to perform a cookie login flow instead. Existing `ResourceFactory` approach can be adapted — `LoginAsync` returns a cookie jar instead of a bearer token. Effort is moderate but mechanical.
- **No API for mobile/external consumers** → Choosing Razor Pages-only means external clients cannot consume this service as an API. Mitigation: this is the stated goal; consumers who need an API should use the separate WebAPI template.

## Open Questions

- None — all key decisions resolved by user input and proposal.

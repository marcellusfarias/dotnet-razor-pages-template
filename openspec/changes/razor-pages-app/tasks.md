## 1. Remove JWT and API Infrastructure

- [x] 1.1 Delete `MyBuyingList.Web/Controllers/` directory (AuthController, UsersController)
- [x] 1.2 Delete `MyBuyingList.Infrastructure/Authentication/Services/JwtProvider.cs` and `JwtBearerOptionsSetup.cs`
- [x] 1.3 Delete `MyBuyingList.Infrastructure/Authentication/` JWT-related files and remove `IJwtProvider` interface from Application layer
- [x] 1.4 Delete `MyBuyingList.Application/Features/Auth/` (AuthService, AuthenticateCommand, RefreshTokenCommand, DTOs, validators related to JWT/refresh)
- [x] 1.5 Delete `MyBuyingList.Domain/Entities/RefreshToken.cs` entity
- [x] 1.6 Delete `MyBuyingList.Infrastructure/Persistence/` RefreshToken configuration, repository, and seeding code
- [x] 1.7 Delete `MyBuyingList.Infrastructure/BackgroundServices/RefreshTokenCleanupService.cs`
- [x] 1.8 Remove `RefreshTokenOptions` class and all related configuration binding

## 2. Recreate Database Migrations from Scratch

- [x] 2.1 Delete the entire `MyBuyingList.Infrastructure/Persistence/Migrations/` directory
- [x] 2.2 Remove the `RefreshTokens` navigation property from the `User` entity and its EF Core fluent configuration
- [x] 2.3 Run `dotnet ef migrations add InitialCreate` to generate a single clean initial migration from the updated model
- [ ] 2.4 Verify the migration applies cleanly against a local PostgreSQL instance (`dotnet ef database update`)

## 3. Cookie Authentication Configuration

- [x] 3.1 Remove JWT NuGet packages (`Microsoft.AspNetCore.Authentication.JwtBearer`) from `MyBuyingList.Web.csproj` and `MyBuyingList.Infrastructure.csproj`
- [x] 3.2 Add `CookieAuthOptions` typed options class (`ExpirationMinutes`, `SlidingExpiration`) in `MyBuyingList.Web/Configuration/`
- [x] 3.3 Configure `appsettings.json`: remove `JwtSettings`, add `CookieAuthOptions` section; update `appsettings.Development.json` accordingly
- [x] 3.4 In `Services.cs`, replace `AddAuthentication().AddJwtBearer(...)` with `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(...)` — set `HttpOnly = true`, `SameSite = Strict`, `Secure = true` (always), sliding expiration, login path `/login`, access-denied path `/access-denied`
- [x] 3.7 At login, embed the `isAdmin` claim in the `ClaimsPrincipal` when the user holds the Administrator role, alongside the existing permission and user ID claims
- [x] 3.5 Remove all Swagger/Swashbuckle NuGet packages and configuration from `Services.cs` and `ConfigureApp.cs`
- [x] 3.6 Remove `CurrentUserService` JWT claim extraction and replace with cookie claim extraction (or verify it already reads from `IHttpContextAccessor` claims, which is auth-mechanism-agnostic)

## 4. Data Protection Key Persistence

- [x] 4.1 Add a Docker volume for Data Protection keys in `docker-compose.development.yml` and `docker-compose.production.yml` (e.g. `/root/.aspnet/DataProtection-Keys` mapped to a named volume)
- [x] 4.2 Configure `services.AddDataProtection().PersistKeysToFileSystem(...)` in `Services.cs`, pointing to the mounted volume path
- [ ] 4.3 Add an integration test asserting that a cookie issued before an application restart is still accepted after restart

## 5. Razor Pages Setup

- [x] 5.1 Add `services.AddRazorPages()` in `Services.cs` and replace `app.MapControllers()` with `app.MapRazorPages()` in `ConfigureApp.cs`; remove controller registration
- [x] 5.2 Create `MyBuyingList.Web/Pages/` directory with `_ViewImports.cshtml` and `_Layout.cshtml` (minimal HTML layout with navigation and logout form)
- [x] 5.3 Add `MyBuyingList.Web/Pages/Shared/_ValidationSummary.cshtml` partial for reuse across pages

## 6. Login and Logout Pages

- [x] 6.1 Create `Pages/Login.cshtml` + `Login.cshtml.cs` — form with username/password, anti-forgery token, validation summary
- [x] 6.2 Implement `OnPostAsync` in `LoginModel`: call `AuthService.AuthenticateAsync(...)`, on success call `HttpContext.SignInAsync(...)` with claims principal, redirect to `returnUrl` (validated with `Url.IsLocalUrl`) or `/users`; on failure redisplay with error
- [x] 6.3 Apply `AuthenticationRateLimiterPolicy` to the `OnPostAsync` handler using `[EnableRateLimiting]` attribute
- [x] 6.4 Create `Pages/Logout.cshtml.cs` (POST-only handler): call `HttpContext.SignOutAsync()` and redirect to `/login`

## 7. Users Razor Pages

- [x] 7.1 Create `Pages/Users/Index.cshtml` + `Index.cshtml.cs` — paginated user list; require `UserGetAll` permission
- [x] 7.2 Create `Pages/Users/Create.cshtml` + `Create.cshtml.cs` — create user form; require `UserCreate` permission; on success redirect to `/users`
- [x] 7.3 Create `Pages/Users/ChangePassword.cshtml` + `ChangePassword.cshtml.cs` — change password form for `{id}`; require `UserUpdate` permission; return 404 if user not found; return 403 if non-admin user attempts to change another user's password
- [x] 7.4 Create `Pages/Users/Delete.cshtml` + `Delete.cshtml.cs` — delete confirmation form for `{id}`; require `UserDelete` permission; return 404 if user not found; return 403 if non-admin user attempts to delete another user; display error if admin deletion is attempted

## 8. Access-Denied and Error Pages

- [x] 8.1 Create `Pages/AccessDenied.cshtml` — minimal page shown on 403; display a message and a link back to `/users`
- [x] 8.2 Create `Pages/Error.cshtml` + `Error.cshtml.cs` — generic error page for production; displays no internal details; configure `app.UseExceptionHandler("/error")` in non-development environments and `app.UseDeveloperExceptionPage()` in development

## 9. Security Response Headers

- [x] 9.1 Add a middleware or `app.Use(...)` call in `ConfigureApp.cs` that sets `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, and `Referrer-Policy: strict-origin-when-cross-origin` on every response
- [x] 9.2 Add an integration test asserting all three headers are present on a normal page response and on an error response

## 10. Rate Limiting Adaptation

- [x] 10.1 Update `AuthenticationRateLimiterPolicy` route/endpoint filter to target the `/login` POST handler (remove references to `/api/auth` routes)
- [x] 10.2 Verify `CustomRateLimiterOptions` binding in `appsettings.json` still works end-to-end

## 11. Update Docker Files

- [x] 11.1 Update `Dockerfile` health check `curl` target if path changed (verify `/health` still maps correctly with Razor Pages routing)
- [x] 11.2 Update `docker-compose.development.yml`: remove `JwtSettings__SigningKey` environment variable reference if present; add Data Protection keys volume
- [x] 11.3 Update `docker-compose.production.yml`: remove `JwtSettings__SigningKey` external secret; add Data Protection keys volume; update any Swagger-related comments
- [x] 11.4 Confirm port exposure and HTTPS certificate setup remain valid for Razor Pages (no change expected)

## 12. Update Integration Tests

- [x] 12.1 Update `ResourceFactory` — replace JWT pre-loading with a `LoginAsync()` helper that POSTs to `/login` with credentials and stores the resulting cookie in `HttpClient.DefaultRequestHeaders` / `CookieContainer`
- [x] 12.2 Remove `AuthControllerIntegrationTests` and `UserControllerIntegrationTests` (API endpoint tests are no longer valid)
- [x] 12.3 Add `LoginPageIntegrationTests` — covers: successful login sets cookie, invalid credentials shows error, locked account shows lockout message, rate limit returns 429, logout clears cookie, external `returnUrl` is rejected
- [x] 12.4 Add `UsersPageIntegrationTests` — covers: list page renders, create user, change password, delete user (authorized and unauthorized scenarios), non-existent `{id}` returns 404, non-admin operating on another user's `{id}` returns 403
- [x] 12.5 Update `CorrelationIdMiddlewareIntegrationTests` to make requests to Razor Page routes instead of API routes
- [x] 12.6 Remove JWT-related test utilities (`Utils.LoginAsync` JWT variant, bearer token helpers); update `Constants.cs` with Razor Page URLs
- [x] 12.7 Update `appsettings.IntegrationTests.json` — remove `JwtSettings`, verify `CookieAuthOptions` values are appropriate for tests (short expiration)

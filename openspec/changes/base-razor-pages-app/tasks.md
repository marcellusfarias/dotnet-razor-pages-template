## 1. Project Scaffold

- [ ] 1.1 Create `MyBuyingList.Web` ASP.NET Core Razor Pages project targeting `net8.0`
- [ ] 1.2 Add `MyBuyingList.Web` to `MyBuyingList.sln`
- [ ] 1.3 Create `Pages/Shared/_Layout.cshtml` with navigation (Home, Users, Logout)
- [ ] 1.4 Create `Program.cs` (minimal bootstrap: builder, `AddServices()`, middleware, `app.Run()`)
- [ ] 1.5 Create `Services.cs` with `AddServices()` extension method skeleton
- [ ] 1.6 Create `appsettings.json` with `BackendApi:BaseUrl` defaulting to `http://localhost:5000`
- [ ] 1.7 Verify `dotnet build MyBuyingList.sln` succeeds

## 2. Configuration

- [ ] 2.1 Create `Configuration/BackendOptions.cs` with `required string BaseUrl` property
- [ ] 2.2 Register and validate `BackendOptions` in `Services.cs` (`ValidateDataAnnotations` + `ValidateOnStart`)

## 3. API Client

- [ ] 3.1 Create `Api/BackendApiClient.cs` with typed `HttpClient` constructor injection
- [ ] 3.2 Add methods: `GetUsersAsync`, `GetHealthAsync`, `LoginAsync`, `RefreshTokenAsync`
- [ ] 3.3 Create `Api/AuthTokenHandler.cs` (`DelegatingHandler`) that reads the access token from `IHttpContextAccessor` and attaches `Authorization: Bearer`
- [ ] 3.4 Implement 401 → refresh → retry logic in `AuthTokenHandler`; redirect to `/auth/login` if refresh also fails
- [ ] 3.5 Register `BackendApiClient`, `AuthTokenHandler`, and `IHttpContextAccessor` in `Services.cs` using `AddHttpClient<BackendApiClient>().AddHttpMessageHandler<AuthTokenHandler>()`

## 4. Authentication UI

- [ ] 4.1 Create `Pages/Auth/Login.cshtml` with username + password form
- [ ] 4.2 Create `Pages/Auth/Login.cshtml.cs` page model: `OnGetAsync` (render), `OnPostAsync` (call `LoginAsync`, set cookie, redirect to `/`)
- [ ] 4.3 Configure cookie authentication in `Services.cs` (`AddAuthentication().AddCookie()`); set `LoginPath = "/auth/login"`
- [ ] 4.4 Create logout handler (`Pages/Auth/Logout.cshtml.cs` or a handler action) that calls `SignOutAsync` and redirects to `/auth/login`
- [ ] 4.5 Add `[Authorize]` attribute (or `AuthorizationPolicy`) to all pages except Login
- [ ] 4.6 Verify unauthenticated access to `/users` redirects to `/auth/login`

## 5. Dashboard Page

- [ ] 5.1 Create `Pages/Index.cshtml` and `Pages/Index.cshtml.cs` — dashboard page
- [ ] 5.2 Call `GetHealthAsync` from the page model and bind health status to the view
- [ ] 5.3 Display health status indicator on the page (e.g., "Healthy" / "Unhealthy")

## 6. Users UI

- [ ] 6.1 Create `Pages/Users/Index.cshtml` and `Pages/Users/Index.cshtml.cs`
- [ ] 6.2 Call `GetUsersAsync` from the page model; bind result to the view
- [ ] 6.3 Render users in a table showing at least username
- [ ] 6.4 Display empty-state message when the user list is empty
- [ ] 6.5 Display error message when the API call fails (catch exception / non-2xx)

## 7. Docker & Compose

- [ ] 7.1 Create `MyBuyingList.Web/Dockerfile` (multi-stage: `sdk:8.0` build → `aspnet:8.0-alpine` runtime, expose port 80)
- [ ] 7.2 Add `web` service to `docker-compose.yml` (build context, depends_on `api`, port mapping, `BackendApi__BaseUrl=http://api:5000`)
- [ ] 7.3 Verify `docker compose up` starts both `api` and `web` services without errors
- [ ] 7.4 Verify login → users list flow works end-to-end via Docker Compose

## ADDED Requirements

### Requirement: Razor Pages project exists in solution
A new `MyBuyingList.Web` ASP.NET Core Razor Pages project SHALL exist at `MyBuyingList.Web/` and be included in `MyBuyingList.sln`.

#### Scenario: Project builds successfully
- **WHEN** `dotnet build MyBuyingList.sln` is executed
- **THEN** the build succeeds with no errors including the `MyBuyingList.Web` project

### Requirement: Project targets .NET 8
The `MyBuyingList.Web` project SHALL target `net8.0`.

#### Scenario: Target framework is set
- **WHEN** the `MyBuyingList.Web.csproj` is inspected
- **THEN** `<TargetFramework>net8.0</TargetFramework>` is present

### Requirement: Standard Razor Pages folder structure
The project SHALL follow the standard Razor Pages layout with pages organized by feature.

#### Scenario: Directory structure is correct
- **WHEN** the project directory is inspected
- **THEN** the following paths exist:
  - `Pages/Index.cshtml` (dashboard/home page)
  - `Pages/Auth/Login.cshtml`
  - `Pages/Users/Index.cshtml`
  - `Pages/Shared/_Layout.cshtml`
  - `wwwroot/` (static assets root)

### Requirement: Program.cs bootstraps via Services.cs
`Program.cs` SHALL only contain host builder creation, a single `AddServices()` call, middleware pipeline setup, and `app.Run()`. All DI registration SHALL live in `Services.cs`.

#### Scenario: Program.cs is minimal
- **WHEN** `Program.cs` is inspected
- **THEN** it contains no `services.Add*()` calls directly and delegates to `Services.AddServices()`

### Requirement: Health status displayed on dashboard
The home page (`Pages/Index.cshtml`) SHALL display the current health status retrieved from the API's health endpoint.

#### Scenario: Health check shown on home page
- **WHEN** an authenticated user navigates to `/`
- **THEN** the page displays a health status indicator (e.g., Healthy / Unhealthy) sourced from the backend health API

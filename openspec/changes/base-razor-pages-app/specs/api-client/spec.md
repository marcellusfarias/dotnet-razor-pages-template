## ADDED Requirements

### Requirement: Typed HTTP client registered via IHttpClientFactory
A `BackendApiClient` typed client SHALL be registered via `AddHttpClient<BackendApiClient>()` in `Services.cs`. It SHALL NOT be created with `new HttpClient()`.

#### Scenario: Client is registered correctly
- **WHEN** the DI container is built
- **THEN** `BackendApiClient` is resolvable and backed by `IHttpClientFactory`

### Requirement: Base URL is configurable via typed options
The `BackendApiClient` base address SHALL be sourced from a `BackendOptions` typed options class bound to the `BackendApi` configuration section. It SHALL NOT be hardcoded.

#### Scenario: Base URL comes from configuration
- **WHEN** `BackendApi:BaseUrl` is set in `appsettings.json` or via the `BackendApi__BaseUrl` environment variable
- **THEN** all HTTP requests from `BackendApiClient` use that base URL

### Requirement: JWT token is attached to every API request
A `DelegatingHandler` (`AuthTokenHandler`) SHALL read the current access token from the HTTP context and attach it as an `Authorization: Bearer` header on every outgoing request.

#### Scenario: Authenticated request includes bearer token
- **WHEN** an authenticated user triggers an API call
- **THEN** the outgoing HTTP request contains `Authorization: Bearer <access_token>`

#### Scenario: Unauthenticated request has no authorization header
- **WHEN** no access token is present in the session
- **THEN** the outgoing HTTP request does not include an `Authorization` header

### Requirement: Automatic token refresh on 401
When the API returns a 401 response, `AuthTokenHandler` SHALL call `POST /api/auth/refresh` with the current refresh token, update the cookie with new tokens, and retry the original request once.

#### Scenario: Expired access token is refreshed transparently
- **WHEN** an API call returns 401 and a valid refresh token exists in the session cookie
- **THEN** the handler obtains a new access token via the refresh endpoint, updates the cookie, and retries the original request

#### Scenario: Expired refresh token redirects to login
- **WHEN** an API call returns 401 and the refresh endpoint also returns 401
- **THEN** the session cookie is cleared and the user is redirected to `/auth/login`

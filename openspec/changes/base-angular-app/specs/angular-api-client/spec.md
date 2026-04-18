## ADDED Requirements

### Requirement: Typed HTTP services wrap Angular HttpClient
The system SHALL provide typed Angular services (`AuthApiService`, `UsersApiService`) under `src/app/api/` that wrap Angular's `HttpClient`. All request and response shapes SHALL be defined as TypeScript interfaces in `src/app/api/types.ts`.

#### Scenario: Auth API service exposes login and refresh methods
- **WHEN** a component calls `AuthApiService.login(username, password)`
- **THEN** an HTTP POST is sent to `/api/auth/login` and the response is typed as `AuthResponse`

#### Scenario: Users API service exposes getUsers
- **WHEN** a component calls `UsersApiService.getUsers()`
- **THEN** an HTTP GET is sent to `/api/users` and the response is typed as `User[]`

### Requirement: AuthInterceptor attaches Bearer token to every request
The system SHALL register a functional `HttpInterceptor` (`AuthInterceptor`) that reads the stored access token from `localStorage` and appends `Authorization: Bearer <token>` to every outgoing HTTP request when a token is present.

#### Scenario: Token is present in localStorage
- **WHEN** an HTTP request is made and an access token exists in `localStorage`
- **THEN** the request includes the header `Authorization: Bearer <token>`

#### Scenario: No token in localStorage
- **WHEN** an HTTP request is made and no access token exists in `localStorage`
- **THEN** the request is forwarded without an Authorization header

### Requirement: AuthInterceptor handles 401 with token refresh and single retry
When the backend returns HTTP 401, the `AuthInterceptor` SHALL call `POST /api/auth/refresh` with the stored refresh token, update stored tokens, and replay the original request exactly once. If the refresh call itself returns a non-2xx response, the interceptor SHALL clear stored tokens and redirect the user to `/login`.

#### Scenario: Access token is expired, refresh succeeds
- **WHEN** an HTTP request returns 401 and the stored refresh token is valid
- **THEN** the interceptor calls `POST /api/auth/refresh`, stores the new tokens, retries the original request, and returns the successful response to the caller

#### Scenario: Access token is expired, refresh also fails
- **WHEN** an HTTP request returns 401 and `POST /api/auth/refresh` returns a non-2xx response
- **THEN** the interceptor clears all stored tokens and redirects the browser to `/login`

#### Scenario: Refresh request is not retried on its own 401
- **WHEN** the refresh request itself returns 401
- **THEN** the interceptor does not attempt another refresh, clears tokens, and redirects to `/login`

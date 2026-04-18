## ADDED Requirements

### Requirement: Axios instance is the single HTTP client
The system SHALL export a configured axios instance from `src/api/client.ts`. All API calls throughout the app SHALL use this instance — never raw `fetch` or a second axios instance.

#### Scenario: Client uses configured base URL
- **WHEN** an API call is made via the client
- **THEN** the request URL is prefixed with the value of `VITE_API_BASE_URL`

### Requirement: Access token is attached to every request
The axios instance SHALL include a request interceptor that reads the stored access token and sets the `Authorization: Bearer <token>` header on every outgoing request. If no token is stored, the header SHALL be omitted.

#### Scenario: Authenticated request includes Authorization header
- **WHEN** a valid access token is in storage and an API request is made
- **THEN** the request includes `Authorization: Bearer <token>`

#### Scenario: Unauthenticated request omits Authorization header
- **WHEN** no access token is in storage and an API request is made
- **THEN** the request does not include an `Authorization` header

### Requirement: 401 response triggers a single token refresh attempt
The axios instance SHALL include a response interceptor. When a 401 response is received, the interceptor SHALL:
1. Call `POST /api/auth/refresh` with the stored refresh token
2. Store the new access and refresh tokens
3. Retry the original request once with the new access token
4. If the refresh call itself returns 401, clear stored tokens and redirect to `/login`

The interceptor SHALL NOT enter an infinite retry loop.

#### Scenario: Successful refresh retries original request
- **WHEN** an API call returns 401 and the refresh token is valid
- **THEN** the client refreshes tokens, retries the original request, and returns its response to the caller

#### Scenario: Failed refresh redirects to login
- **WHEN** an API call returns 401 and the refresh call also returns 401
- **THEN** stored tokens are cleared and the browser navigates to `/login`

### Requirement: Typed endpoint functions exist for each API resource
The `src/api/` folder SHALL export typed functions for each endpoint used by the UI: `login`, `refresh`, `getUsers`. Each function SHALL return a typed Promise (no `any` types).

#### Scenario: getUsers returns typed array
- **WHEN** `getUsers()` is called and the API returns a 200
- **THEN** the resolved value is an array of `User` objects matching the API response shape
